﻿using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Domain.Services;

public class BoardService(
    IBoardRepository boardRepository,
    IProjectRepository projectRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Board> CreateBoardAsync(long projectId, string name, string? description, long creatorUserId)
        {
            var project = await projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new NotFoundException(nameof(Project), projectId);
            }

            //Proveriti validacije za Role
            var projectMember = project.ProjectMembers.SingleOrDefault(pm => pm.UserId == creatorUserId && (pm.Role == ProjectRole.Admin || pm.Role == ProjectRole.Owner));
            if (projectMember == null)
            {
                throw new AccessDeniedException("Samo administratori ili vlasnici projekta mogu kreirati table.");
            }

            var board = Board.Create(name, description, projectId, creatorUserId);

            await boardRepository.AddAsync(board);
            await unitOfWork.CompleteAsync(); // Ovo će automatski dispečovati domenske događaje

            return board;
        }

        public async Task<Board> UpdateBoardDetailsAsync(long boardId, string newName, string? newDescription, long modifierUserId)
        {
            var board = await boardRepository.GetByIdAsync(boardId);
            if (board == null)
            {
                throw new NotFoundException(nameof(Board), boardId);
            }

            var boardMember = board.BoardMembers.SingleOrDefault(bm => bm.UserId == modifierUserId && bm.Role == BoardRole.Editor);
            if (boardMember == null)
            {
                throw new AccessDeniedException("Nemate dozvolu za ažuriranje ove table.");
            }

            board.UpdateDetails(newName, newDescription, modifierUserId);

            await boardRepository.UpdateAsync(board);
            await unitOfWork.CompleteAsync();

            return board;
        }

        public async Task AddBoardMemberAsync(long boardId, long userId, BoardRole role, long inviterUserId)
        {
         
            var board = await boardRepository.GetBoardWithDetailsAsync(boardId);
            if (board == null) throw new NotFoundException(nameof(Board), boardId);

            
            var inviterBoardMember = board.BoardMembers.SingleOrDefault(bm => bm.UserId == inviterUserId && bm.Role == BoardRole.Editor);
            if (inviterBoardMember == null) throw new AccessDeniedException("Samo urednici table mogu dodavati članove.");

            var userToAdd = await userRepository.GetByIdAsync(userId);
            if (userToAdd == null) throw new NotFoundException(nameof(User), userId);
            
            board.AddMember(userToAdd, role, inviterUserId);

            await boardRepository.UpdateAsync(board);
            await unitOfWork.CompleteAsync();
        }

        public async Task RemoveBoardMemberAsync(long boardId, long userId, long removerUserId)
        {
            var board = await boardRepository.GetBoardWithDetailsAsync(boardId);
            if (board == null) throw new NotFoundException(nameof(Board), boardId);

           
            var removerBoardMember = board.BoardMembers.SingleOrDefault(bm => bm.UserId == removerUserId && bm.Role == BoardRole.Editor);
            if (removerBoardMember == null) throw new AccessDeniedException("Nemate dozvolu za uklanjanje članova sa ove table.");

           
            board.RemoveMember(userId, removerUserId);

            await boardRepository.UpdateAsync(board);
            await unitOfWork.CompleteAsync();
        }

        public async Task AddColumnToBoardAsync(long boardId, string columnName, long creatorUserId)
        {
            var board = await boardRepository.GetBoardWithDetailsAsync(boardId);
            if (board == null) throw new NotFoundException(nameof(Board), boardId);
            
            var boardMember = board.BoardMembers.SingleOrDefault(bm => bm.UserId == creatorUserId && bm.Role == BoardRole.Editor);
            if (boardMember == null) throw new AccessDeniedException("Nemate dozvolu za dodavanje kolona na ovu tablu.");

            // Odredi redosled nove kolone
            var nextOrder = board.Columns.Count != 0 ? board.Columns.Max(c => c.Order) + 1 : 1;

            
            board.AddColumn(columnName, nextOrder, creatorUserId);

            await boardRepository.UpdateAsync(board);
            await unitOfWork.CompleteAsync();
        }
        
    }