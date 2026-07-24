using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartyService.Domain.Entities
{
    public class Party
    {
        public Guid Id { get; set; }
        public string BirthdayChildName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? BirthdayChildPhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
