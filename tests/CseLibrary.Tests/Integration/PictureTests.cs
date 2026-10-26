using System.Net;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Results;
using CseLibrary.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Integration;

/// <summary>
/// Book covers, author photos and user photos (archive §4.2 CoverImagePath; Session 25): a file under
/// Uploads/{kind}/, its path in the row, the replaced file removed, served only to those allowed.
/// </summary>
[Collection(AppCollection.Name)]
public class PictureTests(AppFactory app)
{
    private const string Password = "Passw0rd!12";
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3];

    private static ImageUpload Upload(byte[] bytes, long? declared = null) => new(new MemoryStream(bytes), declared ?? bytes.Length);
    private string FileOf(string storedPath) => Path.Combine(app.UploadsRoot, storedPath);
    private int FilesIn(ImageKind kind) =>
        Directory.Exists(Path.Combine(app.UploadsRoot, "Uploads", kind.ToString()))
            ? Directory.GetFiles(Path.Combine(app.UploadsRoot, "Uploads", kind.ToString())).Length : 0;
    private static string Unique(string prefix) => prefix + Guid.NewGuid().ToString("N")[..10];

    [Fact]
    public async Task A_cover_is_a_file_under_Uploads_Books_and_replacing_or_removing_it_deletes_the_old_file()
    {
        await app.WithServiceAsync(async sp =>
        {
            var books = sp.GetRequiredService<IBookService>();
            var id = (await books.CreateAsync(new BookFormDto { Title = Unique("Covered ") })).Value;

            Assert.True((await books.SetCoverAsync(id, Upload(Png))).Succeeded);
            var first = (await books.GetDetailsAsync(id))!.CoverImagePath!;
            Assert.Matches(@"^Uploads/Books/[0-9a-f]{32}\.png$", first); // relative to the web project, random name
            Assert.Equal(Png, await File.ReadAllBytesAsync(FileOf(first)));

            Assert.True((await books.SetCoverAsync(id, Upload(Jpeg))).Succeeded);
            var second = (await books.GetDetailsAsync(id))!.CoverImagePath!;
            Assert.EndsWith(".jpg", second);
            Assert.False(File.Exists(FileOf(first))); // the replaced cover is gone

            Assert.True((await books.SetCoverAsync(id, null)).Succeeded);
            Assert.Null((await books.GetDetailsAsync(id))!.CoverImagePath);
            Assert.False(File.Exists(FileOf(second)));
        });
    }

    [Fact]
    public async Task Only_jpg_png_or_webp_up_to_2_MB_is_accepted_whatever_the_upload_claims()
    {
        await app.WithServiceAsync(async sp =>
        {
            var authors = sp.GetRequiredService<IAuthorService>();
            var id = (await authors.CreateAsync(new AuthorFormDto { Name = Unique("Pictured ") })).Value;
            var before = FilesIn(ImageKind.Authors);

            var big = new byte[2 * 1024 * 1024 + 1];
            Jpeg.CopyTo(big, 0);
            foreach (var refused in new[]
                     {
                         Upload("<svg onload=alert(1)>"u8.ToArray()), // script, even if named .png
                         Upload("GIF89a......"u8.ToArray()),
                         Upload(big),
                         Upload(big, declared: 10) // the declared length is the client's word
                     })
                Assert.Equal(ErrorCodes.InvalidState, (await authors.SetPhotoAsync(id, refused)).ErrorCode);

            var webp = "RIFF\0\0\0\0WEBPVP8 "u8.ToArray();
            Assert.True((await authors.SetPhotoAsync(id, Upload(webp))).Succeeded);
            Assert.EndsWith(".webp", (await authors.GetAsync(id))!.PhotoPath);
            Assert.Equal(before + 1, FilesIn(ImageKind.Authors)); // nothing kept from the refused uploads
        });
    }

    [Fact]
    public async Task A_user_created_with_a_photo_keeps_it_and_a_failed_create_leaves_no_file()
    {
        var email = Unique("photo") + "@test.local";
        await app.WithServiceAsync(async sp =>
        {
            var users = sp.GetRequiredService<IUserService>();
            Assert.True((await users.CreateAsync(new CreateUserDto { UserName = email, Email = email, Password = Password, Photo = Upload(Png) })).Succeeded);
            var user = (await sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email))!;
            Assert.Matches(@"^Uploads/Users/[0-9a-f]{32}\.png$", user.PhotoPath);
            Assert.True(File.Exists(FileOf(user.PhotoPath!)));

            var before = FilesIn(ImageKind.Users);
            var taken = await users.CreateAsync(new CreateUserDto { UserName = email, Email = email, Password = Password, Photo = Upload(Png) });
            Assert.False(taken.Succeeded);
            Assert.Equal(before, FilesIn(ImageKind.Users)); // the photo of the refused user was deleted
        });
    }

    [Fact] // archive: access to uploaded files is restricted
    public async Task A_user_photo_is_served_to_its_owner_and_staff_only_and_a_cover_to_any_signed_in_user()
    {
        var (owner, other) = (Unique("own") + "@test.local", Unique("oth") + "@test.local");
        var (ownerPhoto, otherPhoto, cover) = await app.WithServiceAsync(async sp =>
        {
            var users = sp.GetRequiredService<IUserService>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var email in new[] { owner, other })
                Assert.True((await users.CreateAsync(new CreateUserDto { UserName = email, Email = email, Password = Password, Photo = Upload(Png) })).Succeeded);
            var books = sp.GetRequiredService<IBookService>();
            var bookId = (await books.CreateAsync(new BookFormDto { Title = Unique("Served ") })).Value;
            await books.SetCoverAsync(bookId, Upload(Jpeg));
            return ((await userManager.FindByEmailAsync(owner))!.PhotoPath!, (await userManager.FindByEmailAsync(other))!.PhotoPath!,
                (await books.GetDetailsAsync(bookId))!.CoverImagePath!);
        });
        string Url(string stored) => "/Media/" + stored["Uploads/".Length..];

        var member = await app.LoggedInClient(owner, Password); // no role, no permission
        var own = await member.GetAsync(Url(ownerPhoto));
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Equal("image/png", own.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.NotFound, (await member.GetAsync(Url(otherPhoto))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await member.GetAsync(Url(cover))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await member.GetAsync("/Media/Users/..%2F..%2Fappsettings.json")).StatusCode);

        var admin = await app.LoggedInClient(AppFactory.AdminEmail, AppFactory.AdminPassword); // Users.View
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync(Url(otherPhoto))).StatusCode);

        var anonymous = await app.NewClient().GetAsync(Url(cover));
        Assert.NotEqual(HttpStatusCode.OK, anonymous.StatusCode); // sent to the login page
    }
}
