namespace Volts.Application.DTOs.Shift
{
    public class CreateShiftPositionDto
    {
        public string PositionId { get; set; } = string.Empty;
        public int RequiredCount { get; set; } = 1;
    }
}