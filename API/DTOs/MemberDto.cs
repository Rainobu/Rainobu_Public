using System;
using System.Collections.Generic;

namespace API.DTOs
{
    public class MemberDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PhotoUrl { get; set; }
        public string BannerUrl {get;set;}
        public int Age { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int Point { get; set; }
        public string Title { get; set; }
        public ICollection<BannerDto> Banners {get;set;}
        public ICollection<PhotoDto> Photos { get; set; }
        public ICollection<TitleActiveDto> TitleActives { get; set; }
    }
}