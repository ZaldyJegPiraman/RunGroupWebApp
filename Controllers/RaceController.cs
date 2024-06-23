using Microsoft.AspNetCore.Mvc;
using RunGroupWebApp.Data;
using RunGroupWebApp.Interfaces;
using RunGroupWebApp.Models;
using RunGroupWebApp.Repository;
using RunGroupWebApp.Services;
using RunGroupWebApp.ViewModels;

namespace RunGroupWebApp.Controllers
{
    public class RaceController : Controller
    {
        private readonly IRaceRepository _raceRepository;

        private readonly IPhotoService _photoService;

        public IHttpContextAccessor _httpContextAccessor { get; }

        public RaceController(IRaceRepository raceRepository, IPhotoService photoService, IHttpContextAccessor httpContextAccessor)
        {
            _raceRepository = raceRepository;
            _photoService = photoService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var clubs = await _raceRepository.GetAll();
            return View(clubs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var club = await _raceRepository.GetByIdAsync(id);
            return View(club);

        }


        public async Task<IActionResult> Create()
        {
            var curUserId = _httpContextAccessor.HttpContext.User.GetUserId();
            var createRaceViewModel = new CreateRaceViewModel() { AppUserId = curUserId };
            return View(createRaceViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRaceViewModel raceVM)
        {

            if (ModelState.IsValid)
            {
                var result = await _photoService.AddPhotoAsync(raceVM.Image);

                var race = new Race()
                {
                    Title = raceVM.Title,
                    Description = raceVM.Description,
                    Image = result.Url.ToString(),
                    Address = new Address()
                    {
                        Street = raceVM.Address.Street,
                        State = raceVM.Address.State,
                        City = raceVM.Address.City,
                    },
                    Id = raceVM.AppUserId
                };

                _raceRepository.Add(race);
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Photo upload failed");
            }

            return View(raceVM);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var curUserId = _httpContextAccessor.HttpContext.User.GetUserId();
        

            var race = await _raceRepository.GetByIdAsync(id);
            if (race == null) return View("Error");


            var raceVM = new EditRaceViewModel()
            {

                Title = race.Title,
                Description = race.Description,
                AddressId = race.AddressId,
                Address = race.Address,
                URL = race.Image,
                RaceCategory = race.RaceCategory,
                AppUserId =  curUserId
            };


            return View(raceVM);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditRaceViewModel raceVM)
        {

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit club");
                return View("Edit", raceVM);
            }

            var userClub = await _raceRepository.GetByIdAsyncNoTracking(id);
            if (userClub != null)
            {
                try
                {
                    await _photoService.DeletePhotoAsync(userClub.Image);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Could not delete photo");
                    return View(raceVM);
                }




                var photoResult = await _photoService.AddPhotoAsync(raceVM.Image);
                var race = new Race
                {
                    RaceId = id,
                    Title = raceVM.Title,
                    Description = raceVM.Description,
                    Image = photoResult.Url.ToString(),
                    AddressId = raceVM.AddressId,
                    Address = raceVM.Address,
                    Id = raceVM.AppUserId


                };

                _raceRepository.Update(race);


                return RedirectToAction("Index");
            }
            else
            {
                return View(raceVM);
            }
        }


        public async Task<IActionResult> Delete(int id)
        {
            var raceDetails = await _raceRepository.GetByIdAsync(id);
            if (raceDetails == null) return View("Error");
            return View(raceDetails);


        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteRace(int id)
        {
            var raceDetails = await _raceRepository.GetByIdAsync(id);
            if (raceDetails == null) return View("Error");
            _raceRepository.Delete(raceDetails);
            return RedirectToAction("Index");

        }
    }

}
