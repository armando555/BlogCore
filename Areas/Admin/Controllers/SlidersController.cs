using BlogCore.AccesoDatos.Data.Repository;
using BlogCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlogCore.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class SlidersController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public SlidersController(IContenedorTrabajo contenedorTrabajo, IWebHostEnvironment hostingEnvironment)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Slider slider)
        {
            if (ModelState.IsValid)
            {
                string rutaPrincipal = _hostingEnvironment.WebRootPath;
                var archivos = HttpContext.Request.Form.Files;
                if (slider.Id == 0)
                {
                    //nuevo articulo
                    string nombreArchivo = Guid.NewGuid().ToString();
                    var subidas = Path.Combine(rutaPrincipal, @"imagenes\sliders");
                    var extension = Path.GetExtension(archivos[0].FileName);
                    using (var fileStreams = new FileStream(Path.Combine(subidas, nombreArchivo + extension), FileMode.Create))
                    {
                        archivos[0].CopyTo(fileStreams);
                    }
                    slider.UrlImagen = @"\imagenes\sliders\" + nombreArchivo + extension;
                    _contenedorTrabajo.Slider.Add(slider);
                    _contenedorTrabajo.Save();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(slider);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            else
            {
                Slider slider = new Slider();
                slider = _contenedorTrabajo.Slider.Get(id);
                return View(slider);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Slider slider)
        {
            if (ModelState.IsValid)
            {
                string rutaPrincipal = _hostingEnvironment.WebRootPath;
                var archivos = HttpContext.Request.Form.Files;
                var sliderDesdeDb = _contenedorTrabajo.Slider.Get(slider.Id);
                if (archivos.Count() > 0)
                {
                    //Editamos imagen
                    string nombreArchivo = Guid.NewGuid().ToString();
                    var subidas = Path.Combine(rutaPrincipal, @"imagenes\sliders");
                    var extension = Path.GetExtension(archivos[0].FileName);
                    var nuevaExtension = Path.GetExtension(archivos[0].FileName);
                    var rutaImagen = Path.Combine(rutaPrincipal, sliderDesdeDb.UrlImagen.TrimStart('\\'));

                    if (System.IO.File.Exists(rutaImagen))
                    {
                        System.IO.File.Delete(rutaImagen);
                    }
                    //subimos nuevamente el archivo
                    using (var fileStreams = new FileStream(Path.Combine(subidas, nombreArchivo + nuevaExtension), FileMode.Create))
                    {
                        archivos[0].CopyTo(fileStreams);
                    }
                    slider.UrlImagen = @"\imagenes\sliders\" + nombreArchivo + nuevaExtension;
                    /* articuloVM.Articulo.FechaCreacion = DateTime.Now.ToString();
                     _contenedorTrabajo.Articulo.Update(articuloVM.Articulo);
                     _contenedorTrabajo.Save();
                     return RedirectToAction(nameof(Index));*/
                }
                else
                {//aquí la imagen ya existe, entonces debe conservar la que ya existe y no se reemplaza
                    slider.UrlImagen = sliderDesdeDb.UrlImagen;
                }
                _contenedorTrabajo.Slider.Update(slider);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(slider);

        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var sliderDesdeDb = _contenedorTrabajo.Slider.Get(id);
            string rutaDirectorioPrincipal = _hostingEnvironment.WebRootPath;
            var rutaImagen = Path.Combine(rutaDirectorioPrincipal, sliderDesdeDb.UrlImagen.TrimStart('\\'));
            if (System.IO.File.Exists(rutaImagen))
            {
                System.IO.File.Delete(rutaImagen);
            }
            if (sliderDesdeDb == null)
            {
                return Json(new { success = false, message = "Error borrando artículo" });
            }
            _contenedorTrabajo.Slider.Remove(sliderDesdeDb);
            _contenedorTrabajo.Save();
            return Json(new { success = true, message = "Slider eliminado correctamente" });
        }


        #region llamadas a la API
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Slider.GetAll() });
        }
        /*[HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.Articulo.Get(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error borrando el artículo" });
            }
            _contenedorTrabajo.Articulo.Remove(objFromDb);
            _contenedorTrabajo.Save();
            return Json(new { success = true, message = "Artículo borrado correctamente" });
        }*/
        #endregion
    }
}
