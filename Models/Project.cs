using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class Project
    {
        public int Id { get; set; }

        [DisplayName("Company")]
        public int? CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Project Name")]
        public string Name { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Start Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset StartDate { get; set; }

        [DisplayName("End Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset EndDate { get; set; }

        [DisplayName("Project Priority")]
        public int? ProjectPriorityId { get; set; }

        [DisplayName("Archived")]
        public bool Archived { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        [DisplayName("Project Image")]
        public IFormFile ImageFormFile { get; set; }

        [DisplayName("Image Name")]
        public string ImageFileName { get; set; }

        [DisplayName("Project Image")]
        public byte[] ImageFileData { get; set; }

        [DisplayName("File Extension")]
        public string ImageContentType { get; set; }

        //NAVIGATIONAL PROPERTIES//

        public virtual Company Company { get; set; }

        public virtual ProjectPriority ProjectPriority { get; set; }

        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
