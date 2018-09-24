﻿using System.Threading.Tasks;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    public interface IUserService
    {
        ResponseObject Start();
        Assets GetPortfolio();
    }
}
