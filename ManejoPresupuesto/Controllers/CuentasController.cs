﻿using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers
{



    public class CuentasController : Controller
    {


        private readonly IRepositorioTiposCuentas repositorioTipoCuentas;
        private readonly IServicioUsuarios servicioUsuario;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IMapper mapper;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IServicioReportes servicioReportes;

        public CuentasController(IRepositorioTiposCuentas repositorioTipoCuentas,
            IServicioUsuarios servicioUsuario,
            IRepositorioCuentas repositorioCuentas,
            IMapper mapper, IRepositorioTransacciones repositorioTransacciones,
            IServicioReportes servicioReportes)
        {
            this.repositorioTipoCuentas = repositorioTipoCuentas;
            this.servicioUsuario = servicioUsuario;
            this.repositorioCuentas = repositorioCuentas;
            this.mapper = mapper;
            this.repositorioTransacciones = repositorioTransacciones;
            this.servicioReportes = servicioReportes;
        }


        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var cuentaConTipoCuenta = await repositorioCuentas.Buscar(usuarioId);

            var modelo = cuentaConTipoCuenta.GroupBy(x => x.TipoCuenta).Select(grupo => new IndiceCuentasViewModel
            {
                TipoCuenta = grupo.Key,
                Cuentas = grupo.AsEnumerable()
            }).ToList();
            return View(modelo);
        }
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var modelo = new CuentaCreacionViewModel();
            modelo.TiposCuentas = await obtenerTiposCuentas(usuarioId);
            return View(modelo);
        }

        public async Task<IActionResult> Detalle(int id, int mes, int año)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            ViewBag.Cuenta = cuenta.Nombre;

            var modelo = await servicioReportes
                .ObtenerReporteTransaccionesDetalladasPorCuenta(usuarioId, id, mes, año, ViewBag);

            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTipoCuentas.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if (!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await obtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }
            await repositorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");
        }
        private async Task<IEnumerable<SelectListItem>> obtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTipoCuentas.Obtener(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);

            modelo.TiposCuentas = await obtenerTiposCuentas(usuarioId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaEditar)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(cuentaEditar.Id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = await repositorioCuentas.ObtenerPorId(cuentaEditar.TipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuentas.Actualizar(cuentaEditar);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuentas.Borrar(id);
            return RedirectToAction("Index");
        }

    }
}
