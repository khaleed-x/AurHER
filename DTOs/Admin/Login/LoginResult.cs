namespace AurHER.DTOs.Admin
{
    public class LoginResultDto
    {
        public bool Success {get; set;}
        public string? Role {get; set;}

        public string? UserName { get; set;}
        public string? ErrorMessage{get; set;}

    }
}