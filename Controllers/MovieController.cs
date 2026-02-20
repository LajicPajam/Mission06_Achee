using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mission06LajicPajam.Data;
using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Controllers;

public class MovieController(IMovieRepository repository) : Controller
{
    // Main page: show every movie in the collection table.
    public IActionResult Index()
    {
        var movies = repository.GetMovies();
        return View(movies);
    }

    [HttpGet]
    public IActionResult Add()
    {
        // Use the same Razor form for both Add and Edit to avoid duplicate markup.
        PopulateCategories();
        return View("MovieForm", new Movie());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(Movie movie)
    {
        // If validation fails, return the same form so the user keeps entered values.
        if (!ModelState.IsValid)
        {
            PopulateCategories();
            return View("MovieForm", movie);
        }

        // Save the new row and go back to the list.
        repository.AddMovie(movie);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        // Return 404 when an invalid ID is requested instead of showing a blank form.
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
        // On invalid edits, redisplay the form with errors and existing inputs.
        if (!ModelState.IsValid)
        {
            PopulateCategories();
            return View("MovieForm", movie);
        }

        // Save updates and return to the list.
        repository.UpdateMovie(movie);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        // Show details first so the user can confirm deletion.
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
        // Perform delete after confirmation, then return to the list.
        repository.DeleteMovie(id);
        return RedirectToAction(nameof(Index));
    }

    private void PopulateCategories()
    {
        // Populate the category dropdown each time the form is rendered.
        var categories = repository.GetCategories();
        ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
    }
}
