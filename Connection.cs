using Microsoft.Data.SqlClient;
using SchwarzesBrett.Extensions;
using SchwarzesBrett.Models;
using System.Data;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace SchwarzesBrett
{
    [SupportedOSPlatform("windows")]
    public class Connection(IConfiguration config, IHttpContextAccessor http)
    {
        private readonly string _conString = config.GetConnectionString("DefaultConnection")!;

        private WindowsIdentity CurrentIdentity =>
            http.HttpContext?.User.Identity as WindowsIdentity
            ?? throw new InvalidOperationException("Kein Windows-Benutzer angemeldet.");

        private Task<T> RunAsync<T>(Func<SqlConnection, Task<T>> work)
        {
            return WindowsIdentity.RunImpersonatedAsync(CurrentIdentity.AccessToken, async () =>
            {
                await using SqlConnection connection = new SqlConnection(_conString);
                await connection.OpenAsync();
                return await work(connection);
            });
        }

        private Task RunAsync(Func<SqlConnection, Task> work)
        {
            return RunAsync<bool>(async connection =>
            {
                await work(connection);
                return true;
            });
        }

        private SqlCommand Procedure(string key, SqlConnection connection, SqlTransaction? transaction = null)
        {
            return new SqlCommand(SqlCommandHelper.GetCommand(config, key), connection, transaction)
            {
                CommandType = CommandType.StoredProcedure
            };
        }

        // ======================= Inserate lesen =======================

        public Task<List<ListingTile>> ListingsGet() => RunAsync(async connection =>
        {
            List<ListingTile> tiles = new();
            using SqlCommand command = Procedure("ListingsGet", connection);
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tiles.Add(new ListingTile
                {
                    Id = Convert.ToInt32(reader["IDListing"]),
                    Name = Convert.ToString(reader["name"])!,
                    Price = reader["price"] as decimal?,
                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                    StatusName = Convert.ToString(reader["StatusName"])!,
                    CategoryName = Convert.ToString(reader["CategoryName"])!,
                    CategoryID = Convert.ToInt32(reader["CategoryID"]),
                    PriceCategoryName = Convert.ToString(reader["PriceCategoryName"])!,
                    ListingTypeName = Convert.ToString(reader["ListingTypeName"])!,
                    ListingTypeId = Convert.ToInt32(reader["listingTypeID"]),
                    TitleImageId = reader["TitleImageId"] as int?
                });
            }
            return tiles;
        });

        public Task<List<ListingTile>> ListingsGetByUser() => RunAsync(async connection =>
        {
            List<ListingTile> tiles = new();
            using SqlCommand command = Procedure("ListingsGetByUser", connection);
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tiles.Add(new ListingTile
                {
                    Id = Convert.ToInt32(reader["IDListing"]),
                    Name = Convert.ToString(reader["name"])!,
                    Price = reader["price"] as decimal?,
                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                    CategoryName = Convert.ToString(reader["CategoryName"])!,
                    CategoryID = Convert.ToInt32(reader["CategoryID"]),
                    PriceCategoryName = Convert.ToString(reader["PriceCategoryName"])!,
                    ListingTypeName = Convert.ToString(reader["ListingTypeName"])!,
                    ListingTypeId = Convert.ToInt32(reader["listingTypeID"]),
                    TitleImageId = reader["TitleImageId"] as int?,
                    StatusId = Convert.ToInt32(reader["status"]),
                    ValidUntil = Convert.ToDateTime(reader["valid_until"])
                });
            }
            return tiles;
        });

        public Task<Listing?> ListingGetById(int id) => RunAsync<Listing?>(async connection =>
        {
            using SqlCommand command = Procedure("ListingGetById", connection);
            command.Parameters.AddWithValue("@ListingID", id);
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            Listing listing = new Listing
            {
                Id = Convert.ToInt32(reader["IDListing"]),
                Name = Convert.ToString(reader["name"])!,
                Description = Convert.ToString(reader["description"])!,
                Price = reader["price"] as decimal?,
                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                CreatedBy = Convert.ToString(reader["created_by"])!,
                ContactPhone = reader["contact_phone"] as string,
                ContactMail = Convert.ToString(reader["contact_mail"])!,
                ValidUntil = Convert.ToDateTime(reader["valid_until"]),
                ListingType = new ListingType { Id = Convert.ToInt32(reader["listingTypeID"]), Name = Convert.ToString(reader["listingType_Name"])! },
                Category = new Category { Id = Convert.ToInt32(reader["categoryID"]), Name = Convert.ToString(reader["category_Name"])! },
                PriceCategory = new PriceCategory { Id = Convert.ToInt32(reader["priceCategoryID"]), Name = Convert.ToString(reader["priceCategory_Name"])! },
                Status = new Status { Id = Convert.ToInt32(reader["status"]), Name = Convert.ToString(reader["Status_Name"])! }
            };

            await reader.NextResultAsync();
            while (await reader.ReadAsync())
            {
                listing.Attachments.Add(new Attachment
                {
                    Id = Convert.ToInt32(reader["IDAttachment"]),
                    FileType = Convert.ToString(reader["filetype"])!,
                    FileName = reader["filename"] as string,
                    FileSize = Convert.ToInt64(reader["filesize"]),
                    IsTitle = Convert.ToBoolean(reader["is_title"])
                });
            }
            return listing;
        });

        public Task<ListingLookups> LookupsGetAll() => RunAsync(async connection =>
        {
            ListingLookups lookups = new();
            using SqlCommand command = Procedure("LookupsGetAll", connection);
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                lookups.Categories.Add(new Category
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = Convert.ToString(reader["name"])!,
                    Icon = Convert.ToString(reader["icon"])!
                });

            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                lookups.ListingTypes.Add(new ListingType { Id = Convert.ToInt32(reader["Id"]), Name = Convert.ToString(reader["name"])! });

            await reader.NextResultAsync();
            while (await reader.ReadAsync())
                lookups.PriceCategories.Add(new PriceCategory { Id = Convert.ToInt32(reader["Id"]), Name = Convert.ToString(reader["name"])! });

            return lookups;
        });

        // ======================= Inserate schreiben =======================

        public Task<int> ListingCreate(ListingCreateRequest request) => RunAsync(async connection =>
        {
            using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync();

            using SqlCommand command = Procedure("ListingCreate", connection, transaction);
            command.Parameters.AddWithValue("@name", request.Name);
            command.Parameters.AddWithValue("@description", request.Description);
            command.Parameters.AddWithValue("@listingTypeID", request.ListingTypeId);
            command.Parameters.AddWithValue("@categoryID", request.CategoryId);
            command.Parameters.AddWithValue("@priceCategoryID", request.PriceCategoryId);
            command.Parameters.AddWithValue("@price", (object?)request.Price ?? DBNull.Value);
            command.Parameters.AddWithValue("@contact_phone", (object?)request.ContactPhone ?? DBNull.Value);
            command.Parameters.AddWithValue("@contact_mail", request.ContactMail);

            int listingId = (int)(await command.ExecuteScalarAsync())!;

            int imageIndex = 0;
            foreach (var file in request.Attachments)
            {
                bool isImage = file.ContentType.StartsWith("image/");
                bool isTitle = isImage && imageIndex == request.TitleImageIndex;
                if (isImage) imageIndex++;

                await AttachmentAddInternal(connection, transaction, listingId, file, isTitle);
            }

            await transaction.CommitAsync();
            return listingId;
        });

        public Task ListingEdit(ListingEditRequest request) => RunAsync(async connection =>
        {
            using SqlCommand command = Procedure("ListingEdit", connection);
            command.Parameters.AddWithValue("@IDListing", request.Id);
            command.Parameters.AddWithValue("@name", request.Name);
            command.Parameters.AddWithValue("@description", request.Description);
            command.Parameters.AddWithValue("@listingTypeID", request.ListingTypeId);
            command.Parameters.AddWithValue("@categoryID", request.CategoryId);
            command.Parameters.AddWithValue("@priceCategoryID", request.PriceCategoryId);
            command.Parameters.AddWithValue("@price", (object?)request.Price ?? DBNull.Value);
            command.Parameters.AddWithValue("@contact_phone", (object?)request.ContactPhone ?? DBNull.Value);
            command.Parameters.AddWithValue("@contact_mail", request.ContactMail);
            await command.ExecuteNonQueryAsync();
        });

        public Task ListingDelete(int id) => RunAsync(async connection =>
        {
            using SqlCommand command = Procedure("ListingDelete", connection);
            command.Parameters.AddWithValue("@ListingId", id);
            await command.ExecuteNonQueryAsync();
        });

        public Task ListingSetStatus(int id, int statusId) => RunAsync(async connection =>
        {
            using SqlCommand command = Procedure("ListingSetStatus", connection);
            command.Parameters.AddWithValue("@IDListing", id);
            command.Parameters.AddWithValue("@status", statusId);
            await command.ExecuteNonQueryAsync();
        });

        public Task ListingExtend(int id) => RunAsync(async connection =>
        {
            using SqlCommand command = Procedure("ListingExtend", connection);
            command.Parameters.AddWithValue("@IDListing", id);
            await command.ExecuteNonQueryAsync();
        });

        // ======================= Anhänge =======================

        private async Task AttachmentAddInternal(SqlConnection connection, SqlTransaction? transaction,
                                                 int listingId, IFormFile file, bool isTitle = false)
        {
            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            using SqlCommand command = Procedure("AttachmentAdd", connection, transaction);
            command.Parameters.AddWithValue("@ListingID", listingId);
            command.Parameters.AddWithValue("@filename", Path.GetFileName(file.FileName));
            command.Parameters.AddWithValue("@filetype", file.ContentType);
            command.Parameters.AddWithValue("@value", memoryStream.ToArray());
            command.Parameters.AddWithValue("@is_title", isTitle);
            await command.ExecuteNonQueryAsync();
        }

        public Task AttachmentAdd(int listingId, IFormFile file) => RunAsync(connection =>
            AttachmentAddInternal(connection, null, listingId, file));

        public Task AttachmentDelete(int attachmentId) => RunAsync(async connection =>
        {
            using SqlCommand command = Procedure("AttachmentDelete", connection);
            command.Parameters.AddWithValue("@IDAttachment", attachmentId);
            await command.ExecuteNonQueryAsync();
        });

        public Task AttachmentSetTitle(int attachmentId) => RunAsync(async connection =>
        {
            using SqlCommand command = Procedure("AttachmentSetTitle", connection);
            command.Parameters.AddWithValue("@IDAttachment", attachmentId);
            await command.ExecuteNonQueryAsync();
        });

        public Task<(string Filetype, byte[] Value, string? Filename)?> AttachmentGetContent(int attachmentId) =>
            RunAsync<(string, byte[], string?)?>(async connection =>
            {
                using SqlCommand command = Procedure("AttachmentGetContent", connection);
                command.Parameters.AddWithValue("@IDAttachment", attachmentId);
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return null;

                return ((string)reader["filetype"], (byte[])reader["value"], reader["filename"] as string);
            });
    }
}