﻿using BusinessLayerInterfaces.BusinessModels.Recipe;
using BusinessLayerInterfaces.RecipeServices;
using DALInterfaces.Models.Recipe;
using GamerShop.Models.Recipe;
using GamerShop.Models.Users;
using GamerShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamerShop.Controllers
{
	public class RecipeController : Controller
	{

		private readonly IRecipeServices _recipeServices;
		private readonly IReviewServices _reviewServices;
        private readonly IAuthService _authService;

		public RecipeController(IRecipeServices recipeServices, IAuthService authService, IReviewServices reviewServices)
		{
			_recipeServices = recipeServices;
			_authService = authService;
			_reviewServices = reviewServices;
		}

		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}

		[Authorize]
		[HttpGet]
		public IActionResult Show(int page = 1, int perPage = 3)
		{
			var dataFromBl = _recipeServices.GetPaginatorRecipeBlm(page, perPage);
			var addtionalPageNumber = dataFromBl.Count % dataFromBl.PerPage == 0
				? 0
				: 1;

			var availablePages = Enumerable
				.Range(1, dataFromBl.Count / dataFromBl.PerPage + addtionalPageNumber)
				.ToList();

			var currentUserId = _authService.GetCurrentUser().Id;
			var viewModel = new PaginatorRecipeViewModel
			{
				Page = dataFromBl.Page,
				PerPage = dataFromBl.PerPage,
				Count = dataFromBl.Count,
				AvailablePages = availablePages,
				Recipes = dataFromBl
					.Recipes
					.Select(recipeBlm => new ShowRecipeViewModel
					{
						Id = recipeBlm.Id,
						Title = recipeBlm.Title,
						Description = recipeBlm.Title,
						Instructions = recipeBlm.Instructions,
						CookingTime = recipeBlm.CookingTime,
						PreparationTime = recipeBlm.PreparationTime,
						Servings = recipeBlm.Servings,
						DifficultyLevel = recipeBlm.DifficultyLevel,
						Cuisine = recipeBlm.Cuisine,
						IsFavorite = _recipeServices.GetFavoriteByUser(currentUserId).Any(blm => blm.Id == recipeBlm.Id),
						Reviews = _reviewServices.GetRecipeReviews(recipeBlm.Id).Select(reviewBlm => new DisplayReviewViewModel
						{
							Rating = reviewBlm.Rating,
							Text = reviewBlm.Text,
							Date = reviewBlm.Date,
							RecipeId = reviewBlm.RecipeId,
							Username = reviewBlm.Username
						}).ToList()
					})
					.ToList()
			};

			return View(viewModel);
		}

		[Authorize]
		[HttpGet]
		public IActionResult ShowOnePage()
		{

			var currentUserId = _authService.GetCurrentUser().Id;
			var viewModel = _recipeServices.GetAll().Select(recipeBlm => new ShowRecipeViewModel()
			{
				Id = recipeBlm.Id,
				Title = recipeBlm.Title,
				Description = recipeBlm.Title,
				Instructions = recipeBlm.Instructions,
				CookingTime = recipeBlm.CookingTime,
				PreparationTime = recipeBlm.PreparationTime,
				Servings = recipeBlm.Servings,
				DifficultyLevel = recipeBlm.DifficultyLevel,
				Cuisine = recipeBlm.Cuisine,
				IsFavorite = _recipeServices.GetFavoriteByUser(currentUserId).Any(blm => blm.Id == recipeBlm.Id),
				Reviews = _reviewServices.GetRecipeReviews(recipeBlm.Id).Select(reviewBlm => new DisplayReviewViewModel
				{
					Rating = reviewBlm.Rating,
					Text = reviewBlm.Text,
					Date = reviewBlm.Date,
					RecipeId = reviewBlm.RecipeId,
					Username = reviewBlm.Username
                }).ToList()
			}).ToList();

			return View(viewModel);
		}

		[Authorize]
		[HttpGet]
		public IActionResult Add()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Add(RecipeViewModel recipeViewModel)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}

			var currentUserId = _authService.GetCurrentUser().Id;
			var recipeBlm = new RecipeBlm()
			{
				Title = recipeViewModel.Title,
				Description = recipeViewModel.Title,
				Instructions = recipeViewModel.Instructions,
				CookingTime = recipeViewModel.CookingTime,
				PreparationTime = recipeViewModel.PreparationTime,
				Servings = recipeViewModel.Servings,
				DifficultyLevel = recipeViewModel.DifficultyLevel,
				Cuisine = recipeViewModel.Cuisine,
				CreatedByUserId = currentUserId,
				CreatedOn = DateTime.Now
			};

			_recipeServices.Save(recipeBlm);
			return RedirectToAction("Index");
		}


		public IActionResult Remove(int id)
		{
			_recipeServices.Remove(id);
			return RedirectToAction("Show");
		}

		public IActionResult RemoveFavorite(int recipeId)
		{
			var currentUserId = _authService.GetCurrentUser().Id;
			_recipeServices.RemoveFavorite(new FavoriteRecipeBlm()
			{
				RecipeId = recipeId,
				UserId = currentUserId
			});

			return RedirectToAction("Favorites");
		}

		public IActionResult AddFavorite(int recipeId)
		{
			var currentUserId = _authService.GetCurrentUser().Id;
			_recipeServices.AddFavorite(new FavoriteRecipeBlm()
			{
				RecipeId = recipeId,
				UserId = currentUserId
			});
			return RedirectToAction("Show");
		}

		[Authorize]
		[HttpGet]
		public IActionResult Favorites()
		{
			var currentUserId = _authService.GetCurrentUser().Id;
			var viewModel = _recipeServices
			.GetFavoriteByUser(currentUserId)
			.Select(x => new FavoriteRecipeViewModel()
			{
				Id = x.Id,
				Title = x.Title,
				Description = x.Title,
				Instructions = x.Instructions,
				CookingTime = x.CookingTime,
				PreparationTime = x.PreparationTime,
				Servings = x.Servings,
				DifficultyLevel = x.DifficultyLevel,
				Cuisine = x.Cuisine
			}).ToList();

			return View(viewModel);
		}

		[HttpPost]
		public IActionResult SubmitReview(ReviewViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Show");
            }
            var currentUserId = _authService.GetCurrentUser().Id;
			var reviewBlm = new ReviewBlm
			{
				RecipeId = viewModel.RecipeId,
				UserId = currentUserId,
				Rating = viewModel.Rating,
				Text = viewModel.Text,
            };
            _reviewServices.Save(reviewBlm);
			return RedirectToAction("Show");
		}

	}
}
