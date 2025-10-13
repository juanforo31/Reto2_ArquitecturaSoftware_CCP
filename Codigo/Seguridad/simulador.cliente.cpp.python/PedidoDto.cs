using System.ComponentModel.DataAnnotations;

    public class PedidoDto
    {
        [Required]
        public int VendedorId { get; set; }
        [Required]
        public int ClienteId { get; set; }
        [Required]
        public List<PedidoDetalleDto> Detalle { get; set; }

    }
