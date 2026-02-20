using System.ComponentModel.DataAnnotations;

namespace Mission06LajicPajam.Models;

public class Movie
{
    public int MovieId { get; set; }

    // Stores the selected category foreign key. It can be null when no category is chosen.
    public int? CategoryId { get; set; }

    // Main display name for the movie in the list and edit screens.
    [Required]
    public string Title { get; set; } = string.Empty;

    // Enforces realistic movie years and blocks dates before the first known film (1888).
    [Range(1888, 3000, ErrorMessage = "Year must be 1888 or later.")]
    public int Year { get; set; }

    public string? Director { get; set; }

    public string? Rating { get; set; }

    // Tracks whether this entry is an edited version.
    [Required]
    public bool Edited { get; set; }

    public string? LentTo { get; set; }

    // Tracks whether this movie has been copied into Joel's Plex library.
    [Required]
    public bool CopiedToPlex { get; set; }

    public string? Notes { get; set; }
}
