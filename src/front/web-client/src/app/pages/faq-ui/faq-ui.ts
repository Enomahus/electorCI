import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { MatExpansionModule } from '@angular/material/expansion';

interface FaqItem {
  id: string;
  categoryId: string;
  question: string;
  answer: string;
}

interface FaqCategory {
  id: string;
  name: string;
}

@Component({
  selector: 'app-faq-ui',
  imports: [CommonModule, MatExpansionModule],
  templateUrl: './faq-ui.html',
  styleUrl: './faq-ui.scss',
})
export class FaqUi {
  // Déclarations des propriétés textuelles via des signaux immuables
  title = signal<string>('Foire aux questions');
  subtitle = signal<string>('Trouvez les réponses à vos questions les plus fréquentes');
  readonly panelOpenState = signal(false);

  // Données des catégories
  categories = signal<FaqCategory[]>([
    {
      id: 'infos',
      name: 'Informations générales',
    },
    {
      id: 'request',
      name: 'Faire une demande',
    },
    {
      id: 'contact',
      name: 'Comment nous contacter',
    },
  ]);

  allQuestions = signal<(FaqItem & { categoryId: string })[]>([
    // Catégorie: Informations générales (info)
    {
      id: 'q1',
      categoryId: 'infos',
      question:
        "Quelles sont les étapes à suivre pour s'inscrire en ligne sur la liste électorale ?",
      answer: `Pour s'inscrire en ligne sur la liste électorale, il faut:
      <ul>
        <li>Se connecter à la plateforme <a href="https://www.monelection.ci">monelection.ci</a></li>
        <li>Se créer un compte </li>
        <li>Faire une demande d'inscription en remplissant le formulaire avec tous les chanmps requis et la soumettre.</li>
      </ul>
      `,
    },
    {
      id: 'q2',
      categoryId: 'infos',
      question: "Qui peut s'inscrire en ligne sur la liste électorale ?",
      answer:
        "Tout citoyen Ivoirien(ne) majeur peut faire la demande d'inscription en ligne sur la liste électorale.",
    },
    {
      id: 'q3',
      categoryId: 'infos',
      question:
        "Quelle est la procédure à suivre pour s'inscrire en ligne sur la liste électorale ?",
      answer:
        "Pour s'inscrire en ligne sur la liste électorale, il faut se connecter sur la plateforme 'monelection.ci' et se créer un compte puis suivre les étapes indiquées dans la section comment s'inscrire.",
    },

    // Catégorie: Faire une demande (request)
    {
      id: 'q4',
      categoryId: 'request',
      question: 'Comment créer un compte ?',
      answer:
        "Pour créer un compte, cliquez sur le lien 'S'inscrire' sur la page de connexion et remplissez le formulaire d'inscription.",
    },
    {
      id: 'q5',
      categoryId: 'request',
      question: 'Comment réinitialiser mon mot de passe ?',
      answer:
        "Pour réinitialiser votre mot de passe, cliquez sur le lien 'Mot de passe oublié' sur la page de connexion et suivez les instructions pour recevoir un e-mail de réinitialisation.",
    },

    // Catégorie: Comment nous contacter (contact)
    {
      id: 'q6',
      categoryId: 'contact',
      question: 'Comment contacter le support ?',
      answer:
        'Vous pouvez contacter notre support en envoyant un e-mail à <a href="mailto:support@example.com" class="text-indigo-600 hover:underline">support@example.com</a>.',
    },
  ]);

  // Signaux d'état
  selectedCategoryId = signal<string>('request'); // Initialiser avec l'onglet du milieu, comme dans l'image
  openedQuestionId = signal<number | null>(10); // Ouvrir la première question de la catégorie par défaut

  // Signaux calculés (computed)
  filteredQuestions = computed(() => {
    const currentCategoryId = this.selectedCategoryId();
    return this.allQuestions().filter((q) => q.categoryId === currentCategoryId);
  });

  selectedCategoryName = computed(() => {
    const selectedCatId = this.selectedCategoryId();
    const category = this.categories().find((cat) => cat.id === selectedCatId);
    return category ? category.name : null;
  });

  // Méthodes d'interaction
  selectCategory(id: string): void {
    this.selectedCategoryId.set(id);
    //this.openedQuestionId.set(null); // Fermer toutes les questions lors du changement de catégorie
  }

  // toggleQuestion(id: number): void {
  //   if (this.openedQuestionId() === id) {
  //     this.openedQuestionId.set(null);
  //   } else {
  //     this.openedQuestionId.set(id);
  //   }
  // }
}
