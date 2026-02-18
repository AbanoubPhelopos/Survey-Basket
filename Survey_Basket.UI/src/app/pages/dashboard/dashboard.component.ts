import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PollService } from '../../core/services/poll.service';
import { RequestFilters, PollResponse, PagedList } from '../../core/models/poll';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="min-h-screen bg-gray-50 flex flex-col md:flex-row">
      <!-- Sidebar (Desktop) / Header (Mobile) -->
      <aside class="w-full md:w-64 bg-white border-r border-gray-200 flex-shrink-0">
        <div class="h-16 flex items-center px-6 border-b border-gray-100">
          <span class="text-xl font-bold text-primary-700 tracking-tight">Survey Basket</span>
        </div>
        
        <div class="p-4 flex flex-col h-[calc(100vh-4rem)] justify-between">
          <nav class="space-y-1">
            <a routerLink="/dashboard" class="flex items-center gap-3 px-4 py-3 text-sm font-medium text-primary-700 bg-primary-50 rounded-lg group transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
              </svg>
              My Polls
            </a>
            
            <a *ngIf="authService.user()?.roles?.includes('Admin')" routerLink="/users" class="flex items-center gap-3 px-4 py-3 text-sm font-medium text-gray-600 hover:bg-gray-50 hover:text-gray-900 rounded-lg group transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-gray-400 group-hover:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
              Users
            </a>
            <a href="#" class="flex items-center gap-3 px-4 py-3 text-sm font-medium text-gray-600 hover:bg-gray-50 hover:text-gray-900 rounded-lg group transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-gray-400 group-hover:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
              Analytics
            </a>
            <a routerLink="/profile" class="flex items-center gap-3 px-4 py-3 text-sm font-medium text-gray-600 hover:bg-gray-50 hover:text-gray-900 rounded-lg group transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-gray-400 group-hover:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              Settings
            </a>
          </nav>

          <div class="border-t border-gray-100 pt-4">
            <div class="flex items-center gap-3 px-4 py-2 mb-2">
              <div class="h-8 w-8 rounded-full bg-primary-100 flex items-center justify-center text-primary-700 font-bold text-xs">
                {{ getInitials(authService.user()?.firstName, authService.user()?.lastName) }}
              </div>
              <div class="overflow-hidden">
                <p class="text-sm font-medium text-gray-900 truncate">{{ authService.user()?.firstName }} {{ authService.user()?.lastName }}</p>
                <p class="text-xs text-gray-500 truncate">{{ authService.user()?.email }}</p>
              </div>
            </div>
            <button (click)="logout()" class="w-full flex items-center gap-2 px-4 py-2 text-sm text-red-600 hover:bg-red-50 rounded-lg transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
              </svg>
              Sign out
            </button>
          </div>
        </div>
      </aside>

      <!-- Main Content -->
      <main class="flex-1 overflow-y-auto">
        <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          
          <!-- Header -->
          <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-8 gap-4">
            <div>
              <h1 class="text-2xl font-bold text-gray-900">Dashboard</h1>
              <p class="text-sm text-gray-500 mt-1">Manage your surveys and view results</p>
            </div>
            <button routerLink="/polls/new" class="btn-primary sm:w-auto flex items-center gap-2 shadow-lg shadow-primary-500/30">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clip-rule="evenodd" />
              </svg>
              Create New Poll
            </button>
          </div>

          <!-- Empty State -->
          <div *ngIf="polls().items.length === 0" class="flex flex-col items-center justify-center py-16 bg-white rounded-2xl border border-dashed border-gray-300">
            <div class="h-20 w-20 bg-gray-50 rounded-full flex items-center justify-center mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-10 w-10 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
              </svg>
            </div>
            <h3 class="text-lg font-medium text-gray-900">No polls yet</h3>
            <p class="text-gray-500 max-w-sm text-center mt-1">Get started by creating your first poll to gather feedback from your audience.</p>
          </div>

          <!-- Grid Layout -->
          <div *ngIf="polls().items.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div *ngFor="let poll of polls().items" class="bg-white rounded-xl border border-gray-100 shadow-sm hover:shadow-md transition-all duration-300 group relative overflow-hidden flex flex-col h-full">
              <!-- Top Banner / Status -->
              <div class="h-2 w-full" [ngClass]="poll.isPublished ? 'bg-green-500' : 'bg-yellow-400'"></div>
              
              <div class="p-5 flex-1 flex flex-col">
                <div class="flex justify-between items-start mb-2">
                  <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium" 
                        [ngClass]="poll.isPublished ? 'bg-green-50 text-green-700 border border-green-100' : 'bg-yellow-50 text-yellow-700 border border-yellow-100'">
                    {{ poll.isPublished ? 'Live' : 'Draft' }}
                  </span>
                  <div class="relative">
                     <!-- Menu dot placeholder -->
                     <button class="text-gray-400 hover:text-gray-600">
                       <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                         <path d="M6 10a2 2 0 11-4 0 2 2 0 014 0zM12 10a2 2 0 11-4 0 2 2 0 014 0zM16 12a2 2 0 100-4 2 2 0 000 4z" />
                       </svg>
                     </button>
                  </div>
                </div>

                <h3 class="text-lg font-bold text-gray-900 mb-1 line-clamp-1 group-hover:text-primary-600 transition-colors">{{ poll.title }}</h3>
                <p class="text-sm text-gray-500 mb-4 line-clamp-2 flex-1">{{ poll.summary }}</p>

                <div class="border-t border-gray-50 pt-4 mt-auto">
                   <div class="flex items-center text-xs text-gray-400 mb-3">
                     <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                       <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                     </svg>
                     <span>Started: {{ poll.startedAt | date:'mediumDate' }}</span>
                   </div>
                   
                   <div class="flex gap-2">
                     <button class="flex-1 px-3 py-2 bg-gray-50 hover:bg-gray-100 text-gray-700 text-sm font-medium rounded-lg transition-colors text-center border border-gray-200">
                       Edit
                     </button>
                     <button [routerLink]="['/polls', poll.id, 'results']" class="flex-1 px-3 py-2 bg-primary-50 hover:bg-primary-100 text-primary-700 text-sm font-medium rounded-lg transition-colors text-center border border-primary-100">
                       Results
                     </button>
                     <button *ngIf="poll.isPublished" [routerLink]="['/polls', poll.id, 'vote']" class="flex-1 px-3 py-2 bg-green-50 hover:bg-green-100 text-green-700 text-sm font-medium rounded-lg transition-colors text-center border border-green-100">
                       Vote
                     </button>
                   </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Pagination (Fancy) -->
          <div *ngIf="polls().totalCount > 0" class="mt-8 flex items-center justify-between border-t border-gray-200 pt-4">
             <div class="text-sm text-gray-500">
               Page <span class="font-medium text-gray-900">{{ polls().pageNumber }}</span> of <span class="font-medium text-gray-900">{{ polls().totalPages }}</span>
             </div>
             <div class="flex gap-2">
                <button [disabled]="!polls().hasPreviousPage" (click)="changePage(polls().pageNumber - 1)" 
                        class="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed shadow-sm transition-all">
                  Previous
                </button>
                <button [disabled]="!polls().hasNextPage" (click)="changePage(polls().pageNumber + 1)" 
                        class="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed shadow-sm transition-all">
                  Next
                </button>
             </div>
          </div>

        </div>
      </main>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  authService = inject(AuthService);
  pollService = inject(PollService);
  
  polls = signal<PagedList<PollResponse>>({
    items: [],
    pageNumber: 1,
    totalPages: 0,
    totalCount: 0,
    hasPreviousPage: false,
    hasNextPage: false
  });

  filters: RequestFilters = {
    pageNumber: 1,
    pageSize: 9, // Grid layout works better with 9 items
    sortColumn: 'CreatedOn',
    sortDirection: 'DESC'
  };

  ngOnInit() {
    this.loadPolls();
  }

  loadPolls() {
    this.pollService.getPolls(this.filters).subscribe(result => {
      this.polls.set(result);
    });
  }

  changePage(page: number) {
    this.filters.pageNumber = page;
    this.loadPolls();
  }

  logout() {
    this.authService.logout();
  }

  getInitials(first?: string, last?: string): string {
    return (first?.[0] || '') + (last?.[0] || '');
  }
}
