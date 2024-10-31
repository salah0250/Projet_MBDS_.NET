using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Gauniv.WebServer.Models;
using System.Threading.Tasks;
using Gauniv.WebServer.Data;

[Authorize(Roles = "Admin")]  // Limiter l'accès aux admins
public class AdminController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    // Liste des utilisateurs
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        return View(users);
    }

    // Créer un utilisateur (affiche le formulaire)
    public IActionResult Create()
    {
        return View();
    }

    // Créer un utilisateur (soumettre le formulaire)
    [HttpPost]
    public async Task<IActionResult> Create(User model, string password)
    {
        if (ModelState.IsValid)
        {
            // Assurez-vous que le UserName est défini si ce n'est pas le cas
            if (string.IsNullOrEmpty(model.UserName))
            {
                model.UserName = model.Email;
            }

            // Normalisez l'email et le username
            model.Email = model.Email?.ToLower();
            model.UserName = model.UserName.ToLower();

            var result = await _userManager.CreateAsync(model, password);
            if (result.Succeeded)
            {
                // Ajoutez l'utilisateur au rôle "User" par défaut
                await _userManager.AddToRoleAsync(model, "User");
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        // Si nous arrivons ici, quelque chose a échoué, redisplay form
        return View(model);
    }

    // Modifier un utilisateur (affiche le formulaire)
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    // Modifier un utilisateur (soumettre le formulaire)
    // Méthode Edit améliorée
    [HttpPost]
    public async Task<IActionResult> Edit(User model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                // Mise à jour des propriétés
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email?.ToLower();
                user.UserName = model.UserName?.ToLower();
                user.EmailConfirmed = model.EmailConfirmed;

                // Vérification si l'email est unique
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null && existingUser.Id != model.Id)
                {
                    ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                    return View(model);
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "L'utilisateur a été modifié avec succès.";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Une erreur est survenue lors de la modification de l'utilisateur.");
            }
        }
        return View(model);
    }


    // Supprimer un utilisateur
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Vérifier si l'utilisateur n'est pas le dernier admin
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            if (adminUsers.Count <= 1)
            {
                TempData["ErrorMessage"] = "Impossible de supprimer le dernier administrateur.";
                return RedirectToAction("Index");
            }
        }

        return View(user);
    }

    // Méthode DeleteConfirmed améliorée
    [HttpPost, ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Vérifier si l'utilisateur n'est pas le dernier admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                if (adminUsers.Count <= 1)
                {
                    TempData["ErrorMessage"] = "Impossible de supprimer le dernier administrateur.";
                    return RedirectToAction("Index");
                }
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "L'utilisateur a été supprimé avec succès.";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression de l'utilisateur.";
        }

        return RedirectToAction("Index");
    }

    // Gérer les rôles
    public async Task<IActionResult> ManageRoles(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

         var model = new ManageUserRolesViewModel
         {
             UserId = user.Id,
             UserName = user.UserName,
             Roles = _roleManager.Roles.ToList()
         };

         foreach (var role in model.Roles)
         {
             if (await _userManager.IsInRoleAsync(user, role.Name))
             {
                 model.AssignedRoles.Add(role.Name);
             }
         }

         return View(model);
     }

     [HttpPost]
     public async Task<IActionResult> ManageRoles(ManageUserRolesViewModel model)
     {
         var user = await _userManager.FindByIdAsync(model.UserId);
         if (user == null)
         {
             return NotFound();
         }

         var userRoles = await _userManager.GetRolesAsync(user);

         // Ajouter les nouveaux rôles
         var result = await _userManager.AddToRolesAsync(user, model.AssignedRoles.Except(userRoles).ToList());
         if (!result.Succeeded)
         {
             ModelState.AddModelError("", "Erreur lors de l'ajout des rôles.");
             return View(model);
         }

         // Supprimer les anciens rôles
         result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(model.AssignedRoles).ToList());
         if (!result.Succeeded)
         {
             ModelState.AddModelError("", "Erreur lors de la suppression des rôles.");
             return View(model);
         }

         return RedirectToAction("Index");
     }
      
}

