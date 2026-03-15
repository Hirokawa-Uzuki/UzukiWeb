namespace UzukiWeb.Models
{
    public class GiaoDichHistory
    {
        public string TenGiaoDich { get; set; } = null!;
        public int SoCoin { get; set; }
        public DateTime NgayGiaoDich { get; set; }
        public string LoaiGiaoDich { get; set; } = null!;
    }
}