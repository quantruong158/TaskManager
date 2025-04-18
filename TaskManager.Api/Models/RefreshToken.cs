using System.Text.Json.Serialization;

namespace TaskManager.Api.Models
{
    public class RefreshToken
    {
        public int TokenId { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public DateTime Created { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? ReasonRevoked { get; set; }
        
        // Computed properties
        [JsonIgnore]
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        
        [JsonIgnore]
        public bool IsActive => Revoked == null && !IsExpired;
    }
}