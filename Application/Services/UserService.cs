using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.entities;
using Domain.Interfaces;
using Domain.Repository;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Service
{
    public class UserService : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecurityService _securityService;

        public UserService(IUserRepository userRepository, ISecurityService securityService)
        {
            _userRepository = userRepository;
            _securityService = securityService;
        }

        public async Task<UserResponseDTO> createUser(UserRequestDTO userRequestDTO)
        {
            try
            {
                if (userRequestDTO == null)
                {
                    return new UserResponseDTO
                    {
                        Message = "Parameters is empty or null",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                string passwordHash = _securityService.HashPassword(userRequestDTO.password);

                var newUser = new User
                {
                    UserId = Guid.NewGuid(),
                    Name =userRequestDTO.name,
                    Email = userRequestDTO.email,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow

                };

                await _userRepository.AddAsync(newUser);

                return new UserResponseDTO
                {
                    Message = "User created successfully",
                    Status = "Success",
                    Data = new UserData
                    {
                        Name = userRequestDTO.name,
                        Email = userRequestDTO.email,
                        CreatedAt = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {

                return new UserResponseDTO
                {
                    Message = $"An error occurred: {ex.Message}",
                    Status = "error",
                    Data = null
                };
            }
            
        }
    }
}
