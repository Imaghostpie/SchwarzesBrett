using SchwarzesBrett.Models;
using SchwarzesBrett.Extensions;
using System.Runtime.Versioning;
using Microsoft.Data.SqlClient;

namespace SchwarzesBrett
{
    [SupportedOSPlatform("windows")]
    public class Connection(IConfiguration config)
    {
        private string _conString = config.GetConnectionString("DefaultConnection")!;
        public async Task<List<ListingTile>> ListingsGet()
        {
            List<ListingTile> tiles = new();
            using SqlConnection connection = new SqlConnection(_conString);
            using SqlCommand command = new SqlCommand(Extensions.SqlCommandHelper.GetCommand(config, "ListingsGet"), connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;

            await connection.OpenAsync();

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
                    PriceCategoryName = Convert.ToString(reader["PriceCategoryName"])!,
                    ListingTypeName = Convert.ToString(reader["ListingTypeName"])!,
                    TitleImageId = reader["TitleImageId"] as int?
                });
            }

            return tiles;
        }
        public async Task<List<ListingTile>> ListingsGetByUser(string? username = null)
        {
            List<ListingTile> tiles = new();
            using SqlConnection connection = new SqlConnection(_conString);
            using SqlCommand command = new SqlCommand(Extensions.SqlCommandHelper.GetCommand(config, "ListingsGetByUser"), connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            if (username != null) command.Parameters.AddWithValue("@username", username);
            await connection.OpenAsync();
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
                    PriceCategoryName = Convert.ToString(reader["PriceCategoryName"])!,
                    ListingTypeName = Convert.ToString(reader["ListingTypeName"])!,
                    TitleImageId = reader["TitleImageId"] as int?
                });
            }

            return tiles;
        }
        public async Task<Listing?> ListingGetById(int id)
        {
            using SqlConnection connection = new SqlConnection(_conString);
            using SqlCommand command = new SqlCommand(SqlCommandHelper.GetCommand(config, "ListingGetById"), connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@ListingID", id);

            await connection.OpenAsync();
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
                ListingType = new ListingType
                {
                    Id = Convert.ToInt32(reader["listingTypeID"]),
                    Name = Convert.ToString(reader["listingType_Name"])!
                },
                Category = new Category {
                    Id = Convert.ToInt32(reader["categoryID"]),
                    Name = Convert.ToString(reader["category_Name"])!
                },
                PriceCategory = new PriceCategory
                {
                    Id = Convert.ToInt32(reader["priceCategoryID"]),
                    Name = Convert.ToString(reader["priceCategory_Name"])!
                },
                Status = new Status
                {
                    Id = Convert.ToInt32(reader["status"]),
                    Name = Convert.ToString(reader["Status_Name"])!
                }
            };

            await reader.NextResultAsync();
            while (await reader.ReadAsync())
            {
                listing.Attachments.Add(new Attachment
                {
                    Id = Convert.ToInt32(reader["IDAttachment"]),
                    FileType = Convert.ToString(reader["filetype"])!,
                    FileName = reader["filename"] as string
                });
            }

            return listing;
        }
        public async Task ListingEdit(ListingEditRequest request)
        {
            using SqlConnection connection = new SqlConnection(_conString);
            using SqlCommand command = new SqlCommand(SqlCommandHelper.GetCommand(config, "ListingEdit"), connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IDListing", request.Id);
            command.Parameters.AddWithValue("@name", request.Name);
            command.Parameters.AddWithValue("@description", request.Description);
            command.Parameters.AddWithValue("@listingTypeID", request.ListingTypeId);
            command.Parameters.AddWithValue("@categoryID", request.CategoryId);
            command.Parameters.AddWithValue("@priceCategoryID", request.PriceCategoryId);
            command.Parameters.AddWithValue("@price", (object?)request.Price ?? DBNull.Value);
            command.Parameters.AddWithValue("@contact_phone", (object?)request.ContactPhone ?? DBNull.Value);
            command.Parameters.AddWithValue("@contact_mail", request.ContactMail);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
        public async Task ListingDelete(int id)
        {
            using SqlConnection connection = new SqlConnection(_conString);
            using SqlCommand command = new SqlCommand(SqlCommandHelper.GetCommand(config, "ListingDelete"), connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@ListingId", id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task ListingSetStatus(int id, Status status)
        {
            using SqlConnection connection = new SqlConnection(_conString);
            using SqlCommand command = new SqlCommand(SqlCommandHelper.GetCommand(config, "ListingSetStatus"), connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IDListing", id);
            command.Parameters.AddWithValue("@status", status.Id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task ListingExtend(int id)
        {
            using SqlConnection connection = new SqlConnection(_conString);
            using SqlCommand command = new SqlCommand(SqlCommandHelper.GetCommand(config, "ListingExtend"), connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IDListing", id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
        public async Task<int> ListingCreate(ListingCreateRequest request)
        {

            using SqlConnection connection = new SqlConnection(_conString);
            await connection.OpenAsync();
            using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync();

            using SqlCommand command = new SqlCommand(SqlCommandHelper.GetCommand(config, "ListingCreate"), connection, transaction);
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@name", request.Name);
            command.Parameters.AddWithValue("@description", request.Description);
            command.Parameters.AddWithValue("@listingTypeID", request.ListingTypeId);
            command.Parameters.AddWithValue("@categoryID", request.CategoryId);
            command.Parameters.AddWithValue("@priceCategoryID", request.PriceCategoryId);
            command.Parameters.AddWithValue("@price", (object?)request.Price ?? DBNull.Value);
            command.Parameters.AddWithValue("@contact_phone", (object?)request.ContactPhone ?? DBNull.Value);
            command.Parameters.AddWithValue("@contact_mail", request.ContactMail);

            int listingId = (int)(await command.ExecuteScalarAsync())!;

            foreach (var file in request.Attachments)
                await AttachmentAddInternal(connection, transaction, listingId, file);

            await transaction.CommitAsync();
            return listingId;
        }

        private async Task AttachmentAddInternal(SqlConnection connection, SqlTransaction? transaction, int listingId, IFormFile file)
        {
            using MemoryStream memoryStream = new();
            await file.CopyToAsync(memoryStream);

            using SqlCommand command = new(SqlCommandHelper.GetCommand(config, "AttachmentAdd"), connection, transaction);
            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@ListingID", listingId);
            command.Parameters.AddWithValue("@filename", Path.GetFileName(file.FileName));
            command.Parameters.AddWithValue("@filetype", file.ContentType);
            command.Parameters.AddWithValue("@value", memoryStream.ToArray());

            await command.ExecuteNonQueryAsync();
        }

        public async Task AttachmentAdd(int listingId, IFormFile file)
        {
            using SqlConnection connection = new SqlConnection(_conString);
            await connection.OpenAsync();
            await AttachmentAddInternal(connection, null, listingId, file);
        }

        public async Task AttachmentDelete(int attachmentId)
        {
            using SqlConnection connection = new SqlConnection(_conString);
            using SqlCommand command = new SqlCommand(SqlCommandHelper.GetCommand(config, "AttachmentDelete"), connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IDAttachment", attachmentId);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<(string Filetype, byte[] Value, string? Filename)?> AttachmentGetContent(int attachmentId)
        {
            using SqlConnection connection = new SqlConnection(_conString);
            using SqlCommand command = new SqlCommand(SqlCommandHelper.GetCommand(config, "AttachmentGetContent"), connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IDAttachment", attachmentId);

            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return ((string)reader["filetype"], (byte[])reader["value"], reader["filename"] as string);
        }
    }
}
