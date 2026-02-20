using Microsoft.AspNetCore.Mvc;
using Mission06LajicPajam.Data;
using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Controllers;

public class MovieController(IMovieRepository repository) : Controller
{
    [HttpGet]
    public IActionResult Add()
    {
        // Render an empty form for a new movie entry.
        return View(new Movie());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(Movie movie)
    {
        // Return the form with validation messages if any required fields are missing.
        if (!ModelState.IsValid)
        {
            return View(movie);
        }

        // Persist the movie and show the confirmation page.
        repository.AddMovie(movie);
        return View("Confirmation", movie);
    }
}
