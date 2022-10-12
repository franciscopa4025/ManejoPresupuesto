using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositoriosTipoCuenta;
        private readonly IServicioUsuarios servicioUsuario;

        public TiposCuentasController(IRepositorioTiposCuentas repositoriTipoCuenta, IServicioUsuarios servicioUsuario)
        {
            this.repositoriosTipoCuenta = repositoriTipoCuenta;
            this.servicioUsuario = servicioUsuario;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var TiposCuentas = await repositoriosTipoCuenta.Obtener(usuarioId);
            return View(TiposCuentas);
        }
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {

            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = servicioUsuario.ObtenerUsuarioId();
            var yaExisteTipoCuenta =
                await repositoriosTipoCuenta.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);
            if (yaExisteTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe.");
            }
            await repositoriosTipoCuenta.Crear(tipoCuenta);
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var usuarioid = servicioUsuario.ObtenerUsuarioId();
            var tipoCuenta = await repositoriosTipoCuenta.ObtenerPorId(id, usuarioid);

            if (tipoCuenta is null)
            {
                return RedirectToAction("Noencontrado", "Home");

            }
            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<ActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioid = servicioUsuario.ObtenerUsuarioId();
            var tipoCuetaExiste = await repositoriosTipoCuenta.ObtenerPorId(tipoCuenta.Id, usuarioid);

            if (tipoCuetaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");

            }

            await repositoriosTipoCuenta.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tipoCuenta = await repositoriosTipoCuenta.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Index");
            }
            return View(tipoCuenta);
        }
        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tipoCuenta = await repositoriosTipoCuenta.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Index");
            }
            await repositoriosTipoCuenta.Borrar(id);
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> VerificarExistenteTipoCuenta(string nombre,int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var yaExistenteTipoCuenta = await repositoriosTipoCuenta.Existe(nombre, usuarioId,id);

            if (yaExistenteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }
            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tiposCuentas = await repositoriosTipoCuenta.Obtener(usuarioId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if (idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenados = ids.Select((valor, indice) =>
            new TipoCuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();

            await repositoriosTipoCuenta.Ordenar(tiposCuentasOrdenados);

            return Ok();
        }
    }
}
