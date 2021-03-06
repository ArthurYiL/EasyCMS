﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Atlass.Framework.Common;
using Atlass.Framework.Core.Base;
using Atlass.Framework.Core.BigFile;
using Atlass.Framework.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Atlass.Framework.Web.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        [HttpPost("Uploadimg")]
        public ActionResult Uploadimg()
        {
            var result = new ResultAdaptDto();
            //long size = 0;
            var files = Request.Form.Files;
            if (files.Count == 0)
            {
                result.status = false;
                result.msg = "没有文件信息";
                return Content(result.ToJson());
            }
            string url = $"/upfiles/images/{DateTime.Now.ToString("yyyyMMdd")}";
            var folder = GlobalParamsDto.WebRoot + url;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var file = files[0];
            var filename = ContentDispositionHeaderValue
                .Parse(file.ContentDisposition)
                .FileName
                .Trim('"');
            int index = filename.LastIndexOf('.');
            string extName = filename.Substring(index);
            string guidstr = Guid.NewGuid().ToString("N");
            string guidFileName = guidstr + extName;
            //这个hostingEnv.WebRootPath就是要存的地址可以改下
            filename = $"{folder}/{guidFileName}";
            using (FileStream fs = System.IO.File.Create(filename))
            {
                file.CopyTo(fs);
                fs.Flush();
            }
            var firstFileInfo = new FileInfo(filename);
            if (firstFileInfo.Length > 200 * 1024)
            {
                string compressFileName = IdWorkerHelper.GenObjectId() + extName;
                string compressFile = $"{folder}/{compressFileName}";
                ImageUtilities.CompressImage(filename, compressFile, 90, 200);
                guidFileName = compressFileName;
            }
            string imgurl = $"{ url}/{guidFileName}";
            result.data.Add("url", imgurl);
            return Content(result.ToJson());
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        [HttpPost("UploadLogo")]
        public ActionResult UploadLogo(int imageType)
        {
            var result = new ResultAdaptDto();
            //long size = 0;
            var files = Request.Form.Files;
            if (files.Count == 0)
            {
                result.status = false;
                result.msg = "没有文件信息";
                return Content(result.ToJson());
            }
            string url = $"/static/images";
            if (imageType == 2)
            {
                url = "";
            }
            var folder = GlobalParamsDto.WebRoot + url;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var file = files[0];
            var filename = ContentDispositionHeaderValue
                .Parse(file.ContentDisposition)
                .FileName
                .Trim('"');
            int index = filename.LastIndexOf('.');
            string extName = filename.Substring(index);
            string guidFileName = "logo" + extName;
            if (imageType == 2)
            {
                guidFileName = "favicon.ico";
            }
            //这个hostingEnv.WebRootPath就是要存的地址可以改下
            filename = $"{folder}/{guidFileName}";
            using (FileStream fs = System.IO.File.Create(filename))
            {
                file.CopyTo(fs);
                fs.Flush();
            }
            string imgurl = $"{ url}/{guidFileName}";
            result.data.Add("url", imgurl);
            return Content(result.ToJson());
        }
        #region 分片上传文件，可断点续传

        /// <summary>
        /// 保存文件或者分块
        /// </summary>
        /// <param name="md5">文件md5</param>
        /// <param name="chunk">分块号</param>
        /// <param name="chunks">分块总数</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveFile(string md5, int? chunk, int? chunks)
        {
            var tempDir = "UploadTemp"; // 缓存文件夹
            var targetDir = "UploadFile"; // 目标文件夹

            var file = Request.Form.Files[0];

            file.SaveFileOrChunkFile(targetDir, tempDir, md5, chunks, chunk);
            var result = new ResultAdaptDto();
            result.data.Add("md5", md5);
            result.data.Add("url", Path.Combine("/", targetDir, file.FileName));
            return Content(result.ToJson());
        }

        /// <summary>
        /// 合并文件
        /// </summary>
        /// <param name="md5">文件md5</param>
        /// <param name="fileName">文件名</param>
        /// <param name="chunks">分块数</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult MergeFile(string md5, string fileName, int chunks)
        {
            var tempDir = "UploadTemp";
            var targetDir = "UploadFile";

            var (res, msg) = tempDir.Merge(targetDir, fileName, md5, chunks);
            var result = new ResultAdaptDto();
           
           
            if (!res)
            {
                result.msg = msg;
                result.status = false;
            }
            else
            {
                result.data.Add("md5", md5);
                result.data.Add("url", Path.Combine("/", targetDir, fileName));
            }

            return Content(result.ToJson());

        }

        /// <summary>
        /// 检查文件或分块是否存在
        /// </summary>
        /// <param name="md5">文件md5</param>
        /// <param name="fileName">文件名</param>
        /// <param name="chunk">分块号</param>
        /// <returns></returns>
        public IActionResult CheckFile(string md5, string fileName, int? chunk)
        {
            var tempDir = "UploadTemp";
            var targetDir = "UploadFile";

            string filePath;

            //分片文件
            if (chunk != null)
            {
                filePath = Path.Combine(tempDir, md5, $"{chunk}.part");
            }
            else
            {
                filePath = Path.Combine(targetDir, fileName);
            }

            var exists = System.IO.File.Exists(filePath);
            var Data = fileName != null && exists ? (object)new { md5 = md5, url = Path.Combine("/", targetDir, fileName) } : null;
            var result = new ResultAdaptDto();
            result.status = exists;
            result.data.Add("file", Data);
            return Content(result.ToJson());
        }
        #endregion
    }
}