using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mission06LajicPajam.Data;
using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Controllers;

public class MovieController(IMovieRepository repository) : Controller
{
    public IActionResult Index()
    {
        var movies = repository.GetMovies();
        return View(movies);
    }

    [HttpGet]
    public IActionResult Add()
    {
        PopulateCategories();
        return View("MovieForm", new Movie());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(Movie movie)
    {
        if (!ModelState.IsValid)
        {
            PopulateCategories();
            return View("MovieForm", movie);
        }

        repository.AddMovie(movie);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var movie = repository.GetMovieById(id);
        if (movie is null)
        {
            return NotFound();
        }

        PopulateCategories();
        return View("MovieForm", movie);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Movie movie)
    {
        if (!ModelState.IsValid)
        {
            PopulateCategories();
            return View("MovieForm", movie);
        }

        repository.UpdateMovie(movie);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var movie = repository.GetMovieById(id);
        if (movie is null)
        {
            return NotFound();
        }

        return View(movie);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        repository.DeleteMovie(id);
        return RedirectToAction(nameof(Index));
    }

    private void PopulateCategories()
    {
        var categories = repository.GetCategories();
        ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
    }
}
