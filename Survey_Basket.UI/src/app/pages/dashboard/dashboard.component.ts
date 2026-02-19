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
    <div class="min-h-screen bg-[#F8FAFC] flex flex-col md:flex-row font-sans text-slate-900 text-left">
      <!-- SIDEBAR -->
      <aside class="w-full md:w-72 bg-white border-r border-slate-200 flex-shrink-0 flex flex-col shadow-sm z-20">
        <div class="h-20 flex items-center px-8">
          <div class="bg-indigo-600 p-2 rounded-xl mr-3 shadow-lg shadow-indigo-200">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
            </svg>
          </div>
          <span class="text-xl font-black tracking-tight text-slate-800">SurveyBasket</span>
        </div>
        
        <div class="flex-1 px-4 py-6 overflow-y-auto">
          <nav class="space-y-8">
            <!-- GENERAL SECTION -->
            <div>
              <p class="px-4 text-[10px] font-bold text-slate-400 uppercase tracking-[0.2em] mb-4 text-left">Main Menu</p>
              <div class="space-y-1 text-left">
                <a routerLink="/dashboard" routerLinkActive="bg-indigo-50 text-indigo-700" [routerLinkActiveOptions]="{exact: true}"
                   class="flex items-center gap-3 px-4 py-3 text-sm font-bold text-slate-600 hover:bg-slate-50 hover:text-indigo-600 rounded-2xl transition-all group">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 opacity-50 group-hover:opacity-100" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" /></svg>
                  {{ isAdmin() ? 'Management' : 'Available Surveys' }}
                </a>
              </div>
            </div>

            <!-- MANAGEMENT SECTION (Forced Visible) -->
            <div>
              <p class="px-4 text-[10px] font-bold text-slate-400 uppercase tracking-[0.2em] mb-4 text-left">Administration</p>
              <div class="space-y-1 text-left">
                <a routerLink="/users" routerLinkActive="bg-indigo-50 text-indigo-700"
                   class="flex items-center gap-3 px-4 py-3 text-sm font-bold text-slate-600 hover:bg-slate-50 hover:text-indigo-600 rounded-2xl transition-all group">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 opacity-70 group-hover:opacity-100" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" /></svg>
                  User Accounts
                </a>
              </div>
            </div>
            </div>

            <!-- PERSONAL SECTION -->
            <div class="text-left">
              <p class="px-4 text-[10px] font-bold text-slate-400 uppercase tracking-[0.2em] mb-4">Settings</p>
              <div class="space-y-1 text-left">
                <a routerLink="/profile" routerLinkActive="bg-indigo-50 text-indigo-700"
                   class="flex items-center gap-3 px-4 py-3 text-sm font-bold text-slate-600 hover:bg-slate-50 hover:text-indigo-600 rounded-2xl transition-all group">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 opacity-50 group-hover:opacity-100" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" /></svg>
                  My Profile
                </a>
              </div>
            </div>
          </nav>
        </div>

        <!-- FOOTER / USER CARD -->
        <div class="p-6 bg-slate-50/50 border-t border-slate-100 text-left">
          <div class="flex items-center gap-3 p-3 rounded-2xl bg-white border border-slate-100 shadow-sm mb-4">
            <div class="h-10 w-10 rounded-xl bg-indigo-600 flex items-center justify-center text-white font-black text-xs shadow-md shadow-indigo-100">
              {{ getInitials(authService.user()?.firstName, authService.user()?.lastName) }}
            </div>
            <div class="overflow-hidden">
              <p class="text-sm font-black text-slate-800 truncate leading-tight">{{ authService.user()?.firstName }} {{ authService.user()?.lastName }}</p>
              <span class="text-[9px] font-black text-indigo-600 uppercase tracking-widest bg-indigo-50 px-2 py-0.5 rounded-full">{{ isAdmin() ? 'ADMINISTRATOR' : 'MEMBER' }}</span>
            </div>
          </div>
          <button (click)="logout()" class="w-full py-3 flex items-center justify-center gap-2 text-sm font-bold text-red-500 hover:bg-red-50 rounded-2xl transition-all">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" /></svg>
            Sign out
          </button>
        </div>
      </aside>

      <!-- MAIN CONTENT -->
      <main class="flex-1 overflow-y-auto px-6 py-10 md:px-12 text-left">
        <div class="max-w-6xl mx-auto">
          
          <!-- ADMIN VIEW -->
          <ng-container *ngIf="isAdmin()">
            <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-12 gap-6">
              <div>
                <h1 class="text-4xl font-black text-slate-900 tracking-tight">Poll Center</h1>
                <p class="text-slate-500 mt-2 font-medium">Control panel for your organizational surveys.</p>
              </div>
              <button routerLink="/polls/new" class="btn-primary flex items-center gap-2 px-8 py-4 rounded-2xl shadow-xl shadow-indigo-500/20 font-black">
                CREATE NEW POLL
              </button>
            </div>

            <!-- Admin Grid -->
            <div *ngIf="polls().items.length > 0" class="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-8">
              <div *ngFor="let poll of polls().items" class="bg-white rounded-[2rem] border border-slate-100 p-8 shadow-sm hover:shadow-2xl hover:border-indigo-100 transition-all duration-500 group flex flex-col h-full relative">
                <div class="absolute top-0 left-12 w-16 h-1" [ngClass]="poll.isPublished ? 'bg-emerald-500' : 'bg-amber-400'"></div>
                
                <div class="mb-6 flex justify-between items-center">
                  <span class="px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border" 
                        [ngClass]="poll.isPublished ? 'bg-emerald-50 text-emerald-600 border-emerald-100' : 'bg-amber-50 text-amber-600 border-amber-100'">
                    {{ poll.isPublished ? 'LIVE' : 'DRAFT' }}
                  </span>
                </div>

                <h3 class="text-2xl font-black text-slate-800 mb-3 leading-tight group-hover:text-indigo-600 transition-colors">{{ poll.title }}</h3>
                <p class="text-sm text-slate-500 mb-8 line-clamp-3 leading-relaxed flex-1">{{ poll.summary }}</p>
                
                <div class="grid grid-cols-2 gap-3 border-t border-slate-50 pt-6">
                   <button [routerLink]="['/polls', poll.id, 'edit']" class="px-4 py-3 bg-slate-50 hover:bg-slate-100 text-slate-700 text-xs font-black rounded-xl transition-all border border-slate-100">EDIT</button>
                   <button [routerLink]="['/polls', poll.id, 'results']" class="px-4 py-3 bg-indigo-50 hover:bg-indigo-100 text-indigo-700 text-xs font-black rounded-xl transition-all border border-indigo-100">STATS</button>
                   <button (click)="deletePoll(poll.id)" class="col-span-2 py-3 text-red-400 hover:text-red-600 text-[10px] font-black uppercase tracking-widest transition-all">Delete Survey</button>
                </div>
              </div>
            </div>
            
            <!-- Admin Empty State -->
            <div *ngIf="polls().items.length === 0" class="py-32 text-center bg-white rounded-[3rem] border-4 border-dashed border-slate-50 text-slate-400 font-bold">
               No polls created yet.
            </div>
          </ng-container>

          <!-- MEMBER VIEW -->
          <ng-container *ngIf="!isAdmin()">
            <div class="mb-12">
              <h1 class="text-4xl font-black text-slate-900 tracking-tight">Active Surveys</h1>
              <p class="text-slate-500 mt-2 font-medium text-lg">Help us by sharing your thoughts on the topics below.</p>
            </div>

            <div *ngIf="availablePolls().length > 0" class="grid grid-cols-1 md:grid-cols-2 gap-10">
              <div *ngFor="let poll of availablePolls()" class="bg-white rounded-[2.5rem] border border-slate-100 p-10 shadow-sm hover:shadow-2xl hover:-translate-y-2 transition-all duration-500 group">
                <h3 class="text-3xl font-black text-slate-800 mb-4 group-hover:text-indigo-600 transition-colors leading-tight">{{ poll.title }}</h3>
                <p class="text-slate-500 mb-10 leading-relaxed text-lg line-clamp-4">{{ poll.summary }}</p>
                <button [routerLink]="['/polls', poll.id, 'vote']" class="btn-primary w-full py-5 rounded-[1.5rem] shadow-xl shadow-indigo-500/20 font-black text-lg tracking-wider">
                  TAKE SURVEY NOW
                </button>
              </div>
            </div>

            <div *ngIf="availablePolls().length === 0" class="py-32 text-center bg-white rounded-[3rem] border-4 border-dashed border-slate-50 text-slate-400 font-bold">
               Check back later for new surveys!
            </div>
          </ng-container>

        </div>
      </main>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  authService = inject(AuthService);
  pollService = inject(PollService);
  
  polls = signal<PagedList<PollResponse>>({ items: [], pageNumber: 1, totalPages: 0, totalCount: 0, hasPreviousPage: false, hasNextPage: false });
  availablePolls = signal<PollResponse[]>([]);

  filters: RequestFilters = { pageNumber: 1, pageSize: 9, sortColumn: 'CreatedOn', sortDirection: 'DESC' };

  ngOnInit() {
    this.refreshData();
  }

  isAdmin() {
    // FORCE ADMIN MODE FOR ALL USERS FOR DEVELOPMENT
    return true;
  }

  refreshData() {
    if (this.isAdmin()) {
      this.loadPolls();
    } else {
      this.loadCurrentPolls();
    }
  }

  loadPolls() {
    this.pollService.getPolls(this.filters).subscribe(result => this.polls.set(result));
  }

  loadCurrentPolls() {
    this.pollService.getCurrentPolls().subscribe(result => this.availablePolls.set(result));
  }

  changePage(page: number) {
    this.filters.pageNumber = page;
    this.loadPolls();
  }

  deletePoll(id: string) {
    if (confirm('Permanently delete this survey?')) {
      this.pollService.deletePoll(id).subscribe(() => this.loadPolls());
    }
  }

  logout() {
    this.authService.logout();
  }

  getInitials(first?: string, last?: string): string {
    return ((first?.[0] || '') + (last?.[0] || '')).toUpperCase() || 'SA';
  }
}
