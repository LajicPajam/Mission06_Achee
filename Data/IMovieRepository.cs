using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Data;

public interface IMovieRepository
{
    // Creates the Movies table if it does not already exist.
    void InitializeDatabase();
    // Inserts starter movies, but only when the table is empty.
    void SeedFavoriteMovies();
    // Saves one movie submitted from the Add form.
    void AddMovie(Movie movie);
}
