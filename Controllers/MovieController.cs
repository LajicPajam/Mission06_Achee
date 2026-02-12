using Microsoft.AspNetCore.Mvc;
using Mission06LajicPajam.Data;
using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Controllers;

public class MovieController(IMovieRepository repository) : Controller
{
    [HttpGet]
    public IActionResult Add()
    {
        return View(new Movie());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(Movie movie)
    {
        if (!ModelState.IsValid)
        {
            return View(movie);
        }

        repository.AddMovie(movie);
        return View("Confirmation", movie);
    }
}
