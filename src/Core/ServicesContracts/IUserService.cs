﻿namespace Core.ServicesContracts
{
    public interface IUserService
    {
        void Add(string userName);
        void ChangeStatus(int userId, bool isActive);
    }
}