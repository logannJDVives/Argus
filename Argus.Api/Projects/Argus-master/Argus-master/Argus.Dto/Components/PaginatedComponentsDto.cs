using System.Collections.Generic;

namespace Argus.Dto.Components
{
    public class PaginatedComponentsDto
    {
        public List<SoftwareComponentDto> Items           { get; set; }
        public int                        TotalCount      { get; set; }
        public int                        PageNumber      { get; set; }
        public int                        PageSize        { get; set; }
        public int                        TotalPages      { get; set; }
        public bool                       HasNextPage     { get; set; }
        public bool                       HasPreviousPage { get; set; }
    }
}
