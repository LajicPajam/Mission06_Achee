using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mission06LajicPajam.Data;
using Mission06LajicPajam.Models;

namespace Mission06LajicPajam.Controllers;

public class MovieController(IMovieRepository repository) : Controller
{
    // Display the full movie collection.
    public IActionResult Index()
    {
        var movies = repository.GetMovies();
        return View(movies);
    }

    [HttpGet]
    public IActionResult Add()
    {
        // Reuse one form view for both Add and Edit.
        PopulateCategories();
        return View("MovieForm", new Movie());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(Movie movie)
    {
        // Keep user input and validation messages when the form is invalid.
        if (!ModelState.IsValid)
        {
            PopulateCategories();
            return View("MovieForm", movie);
        }

        // Save and return to list so the user can see the new record.
        repository.AddMovie(movie);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        // 404 for invalid IDs so we do not render an empty form.
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
        // Re-render the same form if validation fails.
        if (!ModelState.IsValid)
        {
            PopulateCategories();
            return View("MovieForm", movie);
        }

        // Persist updates and return to the table.
        repository.UpdateMovie(movie);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        // Show a confirmation page before deleting.
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
        // Delete and immediately navigate back to the list.
        repository.DeleteMovie(id);
        return RedirectToAction(nameof(Index));
    }

    private void PopulateCategories()
    {
        // Categories drive the dropdown on the create/edit form.
        var categories = repository.GetCategories();
        ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
    }
}
