using System.ComponentModel.DataAnnotations;

namespace Mission06LajicPajam.Models;

public class Movie
{
    public int MovieId { get; set; }

    // Linked to Categories table (optional in the provided schema).
    public int? CategoryId { get; set; }

    // Required by rubric.
    [Required]
    public string Title { get; set; } = string.Empty;

    // Required by rubric with first-film lower bound.
    [Range(1888, 3000, ErrorMessage = "Year must be 1888 or later.")]
    public int Year { get; set; }

    public string? Director { get; set; }

    public string? Rating { get; set; }

    // Required by rubric.
    [Required]
    public bool Edited { get; set; }

    public string? LentTo { get; set; }

    // Required by rubric.
    [Required]
    public bool CopiedToPlex { get; set; }

    public string? Notes { get; set; }
}
