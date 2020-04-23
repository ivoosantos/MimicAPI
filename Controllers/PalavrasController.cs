using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicAPI.Helpers;
using MimicAPI.Models;
using MimicAPI.Repositories.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MimicAPI.Models.DTO;

namespace MimicAPI.Controllers
{
    [Route("api/palavras")]
    public class PalavrasController : ControllerBase
    {
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;
        public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        
        //APP -- /api/palavras?data=2020-04-14
        [Route("")]
        [HttpGet]
        public ActionResult ObterTodas([FromQuery]PalavraUrlQuery query)//A data será para armazenar no app, para depois o aplicativo atualizar as palavras novas. 
        {
            var item = _repository.ObterPalavras(query);
            if (query.PagNumero > item.Paginacao.TotalPaginas)
            {
                return NotFound();
            }

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(item.Paginacao));

            return Ok(item.ToList());
            //return new JsonResult(_banco.Palavras);
        }

        //WEB -- /api/palavras/1
        //[Route("{id}")]
        [HttpGet("{id}", Name = "ObterPalavra")]
        public ActionResult Obter(int id)
        {
            var obj = _repository.Obter(id);

            if (obj == null)
                return NotFound();

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(obj);
            palavraDTO.Links = new List<LinkDTO>();
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id}), "GET"));
            palavraDTO.Links.Add(new LinkDTO("update", Url.Link("AtualizarPalavra", new { id = palavraDTO.Id}), "PUT"));
            palavraDTO.Links.Add(new LinkDTO("delete", Url.Link("DeletarPalavra", new { id = palavraDTO.Id }), "DELETE"));

            return Ok(palavraDTO);
        }

        // -- /api/palavras(POST: id, nome, ativo, pontuacao, criacao)
        [Route("")]
        [HttpPost]
        public ActionResult Cadastrar([FromBody]Palavra palavra)
        {
            _repository.Cadastrar(palavra);
            return Created($"/api/palavras/{palavra.Id}", palavra);//Created() -> quando dá certo e retorna o objeto direcionando a uma URL
        }

        // -- /api/palavras(POST: id, nome, ativo, pontuacao, criacao)
        //[Route("{id}")]
        [HttpPut("{id}", Name = "AtualizarPalavra")]
        public ActionResult Atualizar(int id, [FromBody]Palavra palavra)
        {
            var obj = _repository.Obter(id);

            if (obj == null)
                return NotFound();

            palavra.Id = id;
            _repository.Atualizar(palavra);

            return Ok();
        }

        // -- /api/palavras/1 (DELETE)
        //[Route("{id}")]
        [HttpDelete("{id}", Name = "DeletarPalavra")]
        public ActionResult Deletar(int id)
        {
            var palavra = _repository.Obter(id);

            if (palavra == null)
                return NotFound();

            _repository.Deletar(id);

            return NoContent();//NoContent() -> quando tem sucesso mas não retorna nenhuma informação
        }

    }
}
