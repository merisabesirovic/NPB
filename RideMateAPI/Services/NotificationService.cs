using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RideMateAPI.Data;
using RideMateAPI.DTOs;
using RideMateAPI.Models;

namespace RideMateAPI.Services
{
	public class NotificationService
	{
		private readonly RideMateDbContext _db;

		public NotificationService(RideMateDbContext db)
		{
			_db = db;
		}

		public async Task<NotificationDto> CreateNotificationAsync(Guid userId, string title, string message)
		{
			var n = new Notification
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				Title = title,
				Message = message,
				IsRead = false,
				CreatedAt = DateTime.UtcNow
			};

			_db.Notifications.Add(n);
			await _db.SaveChangesAsync();
			return MapToDto(n);
		}

		public async Task<List<NotificationDto>> GetMyNotificationsAsync(Guid userId)
		{
			var list = await _db.Notifications.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).ToListAsync();
			return list.Select(MapToDto).ToList();
		}

		public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId)
		{
			var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
			if (n == null) return false;
			n.IsRead = true;
			await _db.SaveChangesAsync();
			return true;
		}

		public async Task<int> MarkAllAsReadAsync(Guid userId)
		{
			var items = await _db.Notifications.Where(x => x.UserId == userId && !x.IsRead).ToListAsync();
			foreach (var n in items) n.IsRead = true;
			await _db.SaveChangesAsync();
			return items.Count;
		}

		private NotificationDto MapToDto(Notification n)
		{
			return new NotificationDto
			{
				Id = n.Id,
				UserId = n.UserId,
				Title = n.Title,
				Message = n.Message,
				IsRead = n.IsRead,
				CreatedAt = n.CreatedAt
			};
		}
	}
}
