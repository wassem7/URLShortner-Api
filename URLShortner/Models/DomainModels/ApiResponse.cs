using System.Net;

namespace URLShortner.Models.DomainModels;

public class ApiResponse
{
    public bool IsSuccess { get; set; }

    public string SuccessMessage { get; set; }
    public HttpStatusCode HttpStatusCode { get; set; }

    public object Result { get; set; }

    public List<string> ErrorMessages { get; set; }
}
