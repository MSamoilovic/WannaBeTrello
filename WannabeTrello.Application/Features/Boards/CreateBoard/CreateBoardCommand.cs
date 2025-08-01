﻿using MediatR;

namespace WannabeTrello.Application.Features.Boards.CreateBoard;

public class CreateBoardCommand : IRequest<long>
{
    public long ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}