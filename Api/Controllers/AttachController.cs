using Api.Common.Const;
using Api.Models.Attach;
using Api.Services;
using Common.Extentions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AttachController : ControllerBase
    {
        private readonly PostServices _postService;
        private readonly UserServices _userService;

        public AttachController(PostServices postService, UserServices userService)
        {
            _postService = postService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<List<MetadataModel>> UploadFiles([FromForm] List<IFormFile> files)
        {
            var res = new List<MetadataModel>();
            foreach (var file in files)
            {
                res.Add(await UploadFile(file));
            }
            return res;
        }
        [HttpGet]
        [Route("{postContentId}")]
        public async Task<FileStreamResult> GetPostContent(Guid postContentId, bool download = false)
            => RenderAttach(await _postService.GetPostContent(postContentId), download);


        [HttpGet]
        [Route("{userId}")]
        public async Task<FileStreamResult> GetUserAvatar(Guid userId, bool download = false)
            => RenderAttach(await _userService.GetUserAvatar(userId), download);


        [HttpGet]
        public async Task<FileStreamResult> GetCurentUserAvatar(bool download = false)
            => await GetUserAvatar(User.GetClaimValue<Guid>(ClaimNames.Id), download);

        [HttpPost]
        private async Task<MetadataModel> UploadFile(IFormFile file)
        {
            var tempPath = Path.GetTempPath();
            var meta = new MetadataModel
            {
                TempId = Guid.NewGuid(),
                Name = file.FileName,
                MimeType = file.ContentType,
                Size = file.Length
            };

            var newPath = Path.Combine(tempPath, meta.TempId.ToString());

            var fileinfo = new FileInfo(newPath);
            if (fileinfo.Exists)
                throw new Exception("File exist");
            else
                if (fileinfo.Directory == null)
                    throw new Exception("Temp is null");
                else
                if (!fileinfo.Directory.Exists)
                    fileinfo.Directory?.Create();

                using (var stream = System.IO.File.Create(newPath))
                {
                    await file.CopyToAsync(stream);
                }
                return meta;
            
        }

        private FileStreamResult RenderAttach(AttachModel attach, bool download)
        {
            var fs = new FileStream(attach.FilePath, FileMode.Open);
            var ext = Path.GetExtension(attach.Name);
            if (download)
                return File(fs, attach.MimeType, $"{attach.Id}{ext}");
            else
                return File(fs, attach.MimeType);

        }
    }
}
