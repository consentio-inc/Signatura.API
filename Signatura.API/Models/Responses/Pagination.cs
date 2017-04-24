namespace Signatura.API.Models.Responses {
    public class Pages {
        public string Next { get; set; }
    }

    public class Pagination {
        public Pages Pages { get; set; }
    }
}
