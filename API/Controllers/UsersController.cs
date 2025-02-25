using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Specifications;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    // [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<AppUser> _membersRepo;
        public UsersController(IUnitOfWork unitOfWork, IMapper mapper,
        
        IPhotoService photoService, IGenericRepository<AppUser> membersRepo)
        {
            _membersRepo = membersRepo;
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _mapper = mapper;
        }

    [HttpGet]
     public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        // var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername());
        userParams.CurrentUsername = User.GetUsername();

        // if (string.IsNullOrEmpty(userParams.Gender))
        //     userParams.Gender = gender == "male" ? "female" : "male";

        var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(users.CurrentPage, users.PageSize,
            users.TotalCount, users.TotalPages);

        return Ok(users);
    }
    
    // [HttpGet]
    // public async Task<ActionResult<Pagination<MemberDto>>> GetUsers([FromQuery] MemberSpecParams memberSpecParams)
    // {
       
    //         var spec = new MemberWithRank(memberSpecParams);

    //         var countSpec = new MemberWithFilterCount(memberSpecParams);

    //         var totalItems = await _membersRepo.CountAsync(countSpec);

    //         var members = await _membersRepo.ListAsync(spec);

    //         var data = _mapper.Map<IReadOnlyList<MemberDto>>(members);

    //         return Ok(new Pagination<MemberDto>(memberSpecParams.PageIndex,
    //             memberSpecParams.PageSize, totalItems, data));
    // }

    [HttpGet("{username}", Name = "GetUser")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        return await _unitOfWork.UserRepository.GetMemberAsync(username);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var username = User.GetUsername();
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        var existKnowAs = await _unitOfWork.UserRepository.GetUserByKnowAs(memberUpdateDto.KnownAs,username);
        if(existKnowAs != null){
            return BadRequest("User KnowAs is taken");
        }
        _mapper.Map(memberUpdateDto, user);

        _unitOfWork.UserRepository.Update(user);

        if (await _unitOfWork.Complete()) return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if (user.Photos.Count == 0)
        {
            photo.IsMain = true;
        }

        user.Photos.Add(photo);

        if (await _unitOfWork.Complete())
        {
            return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
        }
        return BadRequest("Problem addding photo");
    }
    [HttpPost("add-banner")]
    public async Task<ActionResult<PhotoDto>> AddBanner(IFormFile file)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var result = await _photoService.AddBannerAsync(file);

        if (result.Error != null) return BadRequest(result.Error.Message);

        var banner = new Banner
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if (user.Banners.Count == 0)
        {
            banner.IsMain = true;
        }

        user.Banners.Add(banner);

        if (await _unitOfWork.Complete())
        {
            return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<BannerDto>(banner));
        }
        return BadRequest("Problem addding Banner");
    }
    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo.IsMain) return BadRequest("This is already your main photo");

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;

        if (await _unitOfWork.Complete()) return NoContent();

        return BadRequest("Failed to set main photo");
    }
    [HttpPut("set-main-banner/{bannerId}")]
    public async Task<ActionResult> SetMainBanner(int bannerId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var banner = user.Banners.FirstOrDefault(x => x.Id == bannerId);

        if (banner.IsMain) return BadRequest("This is already your main banner");

        var currentMain = user.Banners.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        banner.IsMain = true;

        if (await _unitOfWork.Complete()) return NoContent();

        return BadRequest("Failed to set main banner");
    }
    [HttpPut("set-main-title/{titleId}")]
    public async Task<ActionResult> SetMainTitle(int titleId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var title = user.titleAcitive.FirstOrDefault(x => x.Id == titleId);

        if (title.IsMain) return BadRequest("This is already your main title");

        var currentMain = user.titleAcitive.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        title.IsMain = true;
        //Update User.Title
        user.Title = title.Name;
        _unitOfWork.UserRepository.Update(user);
        if (await _unitOfWork.Complete()) return NoContent();

        return BadRequest("Failed to set main title");
    }
    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null) return NotFound();

        if (photo.IsMain) return BadRequest("You cannot delete your main photo");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await _unitOfWork.Complete()) return Ok();

        return BadRequest("Failed to delete the photo");
    }
    [HttpDelete("delete-banner/{bannerId}")]
    public async Task<ActionResult> DeleteBanner(int bannerId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var banner = user.Banners.FirstOrDefault(x => x.Id == bannerId);

        if (banner == null) return NotFound();

        if (banner.IsMain) return BadRequest("You cannot delete your main photo");

        if (banner.PublicId != null)
        {
            //same function
            var result = await _photoService.DeletePhotoAsync(banner.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Banners.Remove(banner);

        if (await _unitOfWork.Complete()) return Ok();

        return BadRequest("Failed to delete the banner");
    }
    [HttpDelete("delete-title/{titleId}")]
    public async Task<ActionResult> Deletetitle(int titleId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var title = user.titleAcitive.FirstOrDefault(x => x.Id == titleId);

        if (title == null) return NotFound();

        if (title.IsMain) return BadRequest("You cannot delete your main title");

        user.titleAcitive.Remove(title);

        if (await _unitOfWork.Complete()) return Ok();

        return BadRequest("Failed to delete the title");
    }
}
}