﻿using MediatR;
using MyLab.OidcFinisher.Models;

namespace MyLab.OidcFinisher.Features.Finish;

public record FinishCmd(string AuthorizationCode, string? State) : IRequest<FinishResult>;