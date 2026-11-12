using System.ComponentModel.DataAnnotations;

namespace ParamonovAPI.Models
{
    public class CreateProjectDraftDto
    {
        /// <summary>
        /// Название проекта. Необязательно — если пусто, сервер подставит «Новый проект».
        /// </summary>
        [StringLength(255, ErrorMessage = "Название не должно превышать 255 символов")]
        public string Name { get; set; }

        /// <summary>
        /// Краткое описание проекта. Необязательно.
        /// </summary>
        [StringLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов")]
        public string Description { get; set; }
    }
}
