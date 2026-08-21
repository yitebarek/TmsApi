using System.ComponentModel.DataAnnotations;

public class PaymentOption
{
    [Required]
    public required string GateWayurl{get; init;}
    [Range(100, 100000)]
    public decimal MaxDepositBirr{get; init;} 
}

