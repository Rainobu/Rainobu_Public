namespace API.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public bool IsAuthSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public bool Is2StepVerificationRequired { get; set; }
        public string Provider { get; set; }
    }
}