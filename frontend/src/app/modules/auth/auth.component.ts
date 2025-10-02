import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css'],
  imports: [AngularSvgIconModule, RouterOutlet],
})
export class AuthComponent implements OnInit {
  constructor(public themeService: ThemeService) {}

  ngOnInit(): void {}

  get logoSrc(): string {
    return this.themeService.isDark ? 'assets/icons/white_logo.png' : 'assets/icons/dark_logo.png';
  }
}
