import {
  Directive, ElementRef, Input, OnInit, OnDestroy, inject
} from '@angular/core';

@Directive({
  selector:   '[scrollReveal]',
  standalone: true
})
export class ScrollRevealDirective implements OnInit, OnDestroy {

  @Input() revealDelay:     number = 0;
  @Input() revealDirection: 'fadeUp' | 'fadeLeft' | 'fadeRight' | 'fadeIn' = 'fadeUp';

  private el = inject(ElementRef);
  private observer!: IntersectionObserver;

  ngOnInit() {
    const element = this.el.nativeElement as HTMLElement;
    this.setInitialState(element);

    this.observer = new IntersectionObserver(
      (entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            setTimeout(() => this.setVisibleState(element), this.revealDelay);
            this.observer.unobserve(element);
          }
        });
      },
      { threshold: 0.12, rootMargin: '0px 0px -40px 0px' }
    );

    this.observer.observe(element);
  }

  ngOnDestroy() {
    if (this.observer) this.observer.disconnect();
  }

  private setInitialState(el: HTMLElement) {
    el.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
    el.style.opacity    = '0';
    switch (this.revealDirection) {
      case 'fadeUp':    el.style.transform = 'translateY(32px)';  break;
      case 'fadeLeft':  el.style.transform = 'translateX(-32px)'; break;
      case 'fadeRight': el.style.transform = 'translateX(32px)';  break;
      case 'fadeIn':    el.style.transform = 'none';              break;
    }
  }

  private setVisibleState(el: HTMLElement) {
    el.style.opacity   = '1';
    el.style.transform = 'none';
  }
}