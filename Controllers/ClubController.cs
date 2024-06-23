using Microsoft.AspNetCore.Mvc;
using RunGroupWebApp.Data;
using Microsoft.EntityFrameworkCore;
using RunGroupWebApp.Interfaces;
using RunGroupWebApp.Models;
using static System.Reflection.Metadata.BlobBuilder;
using RunGroupWebApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;

namespace RunGroupWebApp.Controllers
{
    public class ClubController : Controller
    {

        private readonly IClubRepository _clubRepository;
        private readonly IPhotoService _photoService;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClubController(IClubRepository clubRepository,IPhotoService photoService, IHttpContextAccessor httpContextAccessor)
        {
            _clubRepository = clubRepository;
            _photoService = photoService;
            _httpContextAccessor = httpContextAccessor;
        }


 
        public async Task<IActionResult> Index()
        {
            var clubs = await _clubRepository.GetAll();
            return View(clubs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            return View(club);

        }

        public async Task<IActionResult> Create()
        {
            var curUserId = _httpContextAccessor.HttpContext.User.GetUserId();
            var createClubViewModel = new CreateClubViewModel() { AppUserId = curUserId };
            return View(createClubViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateClubViewModel clubVM)
        {

            if (ModelState.IsValid)
            {

                var result = await _photoService.AddPhotoAsync(clubVM.Image);

                var club = new Club()
                {
                    Title = clubVM.Title,
                    Description = clubVM.Description,
                    Image = result.Url.ToString(),
                    Address = new Address()
                    {
                        Street = clubVM.Address.Street,
                        State = clubVM.Address.State,
                        City = clubVM.Address.City,
                    },
                    Id = clubVM.AppUserId
                };

              _clubRepository.Add(club);
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Photo upload failed");
            }

            return View(clubVM);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var curUserId = _httpContextAccessor.HttpContext.User.GetUserId();
            var club = await _clubRepository.GetByIdAsync(id);
            if(club==null) return View("Error");


            var clubVM = new EditClubViewModel()
            {

                Title = club.Title,
                Description = club.Description,
                AddressId = club.AddressId,
                Address = club.Address,
                URL = club.Image,
                ClubCategory = club.ClubCategory,
                 AppUserId = curUserId
            };
     

            return View(clubVM);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditClubViewModel clubVM)
        {

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit club");
                return View("Edit", clubVM);
            }

            var userClub = await _clubRepository.GetByIdAsyncNoTracking(id);
            if (userClub != null)
            {
                try
                {
                    await _photoService.DeletePhotoAsync(userClub.Image);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Could not delete photo");
                    return View(clubVM);
                }



        
                var photoResult = await _photoService.AddPhotoAsync(clubVM.Image);
                var club = new Club
                {
                    ClubId = id,
                    Title = clubVM.Title,
                    Description = clubVM.Description,
                    Image = photoResult.Url.ToString(),
                    AddressId = clubVM.AddressId,
                    Address = clubVM.Address,
                    Id = clubVM.AppUserId
                };

                _clubRepository.Update(club);


                return RedirectToAction("Index");
            }
            else
            {
               return View(clubVM);     
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var clubDetails = await _clubRepository.GetByIdAsync(id);
            if(clubDetails == null) return View("Error");
            return View(clubDetails);


        }

        [HttpPost,ActionName("Delete")]
        public async Task<IActionResult> DeleteClub(int id)
        {
            var clubDetails = await _clubRepository.GetByIdAsync(id);
            if (clubDetails == null) return View("Error");
            _clubRepository.Delete(clubDetails);
            return RedirectToAction("Index");

        }
    }
}
