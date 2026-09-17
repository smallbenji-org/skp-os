<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import {
  IconInfoCircle,
  IconFolder,
  IconShield,
  IconHeadset,
  IconHeart,
  IconUpload,
  IconHelpCircle,
  IconAlertTriangle,
  IconExternalLink,
  IconPhone,
  IconChevronDown,
  IconWifi,
  IconClock,
  IconCheck,
  IconMessageReport,
  IconX,
  IconLoader2,
  IconPin,
  IconArrowRight,
} from "@tabler/icons-vue";
import { useInfoEntryStore } from "@/Stores/InfoEntryStore";

const router = useRouter();
const infoEntryStore = useInfoEntryStore();

const isLoading = ref(true);

// Top category navigation
const activeCategory = ref<string>("generelt");

const categories = [
  { id: "generelt", label: "Generelt", targetId: "sec-intro" },
  { id: "projekt", label: "Projekt", targetId: "sec-projekt" },
  { id: "arbejdsmiljo", label: "Arbejdsmiljø", targetId: "sec-arbejdsmiljo" },
  { id: "aflevering", label: "Aflevering", targetId: "sec-aflevering" },
  { id: "faq", label: "FAQ", targetId: "sec-faq" },
];

// Left sidebar in-page anchors
const pageSections = [
  { id: "sec-intro", label: "Introduktion", icon: IconInfoCircle },
  { id: "sec-projekt", label: "Projekt & SOP", icon: IconFolder },
  { id: "sec-regler", label: "Vigtige regler", icon: IconShield },
  { id: "sec-kontakt", label: "Kontakt & hjælp", icon: IconHeadset },
  { id: "sec-arbejdsmiljo", label: "Arbejdsmiljø", icon: IconHeart },
  { id: "sec-aflevering", label: "Aflevering", icon: IconUpload },
  { id: "sec-faq", label: "FAQ", icon: IconHelpCircle },
];

const activeSectionId = ref<string>("sec-intro");

function scrollToSection(sectionId: string, categoryId?: string) {
  if (categoryId) activeCategory.value = categoryId;
  const el = document.getElementById(sectionId);
  if (el) {
    el.scrollIntoView({ behavior: "smooth", block: "start" });
  }
}

// Scroll spy for active section highlight
let observer: IntersectionObserver | null = null;
onMounted(() => {
  const options: IntersectionObserverInit = {
    root: null,
    rootMargin: "-20% 0px -60% 0px",
    threshold: 0,
  };

  observer = new IntersectionObserver((entries) => {
    for (const entry of entries) {
      if (entry.isIntersecting) {
        activeSectionId.value = entry.target.id;
        break;
      }
    }
  }, options);

  pageSections.forEach((sec) => {
    const el = document.getElementById(sec.id);
    if (el) observer?.observe(el);
  });
});

onUnmounted(() => {
  observer?.disconnect();
});

// Dynamic info entries from backend
const infoEntries = computed(() =>
  [...infoEntryStore.INFO_ENTRIES].sort((a, b) => {
    if (a.isPinned !== b.isPinned) return a.isPinned ? -1 : 1;
    return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
  }),
);

function formatDateTime(dateStr?: string | null): string {
  if (!dateStr) return "";
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  return d.toLocaleDateString("da-DK", { day: "2-digit", month: "short", year: "numeric" });
}

// FAQ accordion state
const openFaqIndex = ref<number | null>(null);

function toggleFaq(index: number) {
  openFaqIndex.value = openFaqIndex.value === index ? null : index;
}

const faqs = [
  {
    q: "Hvordan opretter jeg et personligt projekt?",
    a: "Du kan oprette et personligt projekt under 'Mine Projekter' ved at klikke på knappen 'Nyt personligt projekt'. Her angiver du projekttitel, en kort beskrivelse og eventuelt link til et GitHub repository. Herefter er projektet automatisk knyttet til din profil.",
  },
  {
    q: "Kan jeg få mere tid til en aflevering?",
    a: "Frister og tidsplaner aftales altid med din instruktør. Hvis du forudser forsinkelse på grund af tekniske udfordringer eller sygdom, skal du henvende dig i god tid til instruktøren, så I kan justere projektplanen.",
  },
  {
    q: "Hvordan deler jeg mit projekt med min underviser?",
    a: "Dine projekter er automatisk synlige for instruktørerne i SKP OS. Når du afleverer dit projekt via projektets detaljeside, notificeres din instruktør, som herefter kan gennemse koden, logbogen og de tilknyttede bilag.",
  },
  {
    q: "Hvad gør jeg hvis jeg har tekniske problemer?",
    a: "Ved tekniske problemer med netværk, login eller computere kan du henvende dig til IT-support på support.itskp-odense.dk eller skrive en mail til helpdesk@itskp-odense.dk. Du kan også spørge din instruktør i lokalet.",
  },
  {
    q: "Hvor finder jeg skolens regler og retningslinjer?",
    a: "Du finder de vigtigste regler for arbejdstid, fravær og pauser her på informationssiden under sektionen 'Vigtige regler' samt i det detaljerede studie- og ordensreglement.",
  },
  {
    q: "Hvordan optjener og bruger jeg FF-timer?",
    a: "Du optjener 3 FF-timer pr. måned med en maksimal saldo på 37 timer. Minustimer er ikke tilladt. Ved fri trækkes der 7,5 timer mandag–torsdag og 7,0 timer fredag (i alt 37 timer for en hel uge). Brug af FF-timer skal altid aftales med din instruktør senest dagen før.",
  },
  {
    q: "Skal jeg søge læreplads, mens jeg er på skoleophold?",
    a: "Ja. Når du er optaget i skoleoplæringen, er du omfattet af EMMA-kriterierne både i skoleoplæringen og under skoleophold. Du skal have en synlig profil på lærepladsen.dk, søge mindst én stilling ugentligt og uploade dine ansøgninger.",
  },
];

// Detail Modals
const showRulesModal = ref(false);
const showFeedbackModal = ref(false);

// Feedback / Forslagskasse form
const feedbackTitle = ref("");
const feedbackDesc = ref("");
const isSubmittingFeedback = ref(false);
const feedbackSuccess = ref(false);

function submitFeedback() {
  if (!feedbackTitle.value.trim() || !feedbackDesc.value.trim()) return;
  isSubmittingFeedback.value = true;
  setTimeout(() => {
    isSubmittingFeedback.value = false;
    feedbackSuccess.value = true;
    setTimeout(() => {
      feedbackSuccess.value = false;
      feedbackTitle.value = "";
      feedbackDesc.value = "";
      showFeedbackModal.value = false;
    }, 1500);
  }, 600);
}

onMounted(async () => {
  try {
    await infoEntryStore.GET_INFO_ENTRIES();
  } catch {
    // Continue gracefully if backend is offline
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <header class="page-header">
      <div class="header-titles">
        <h1 class="page-title">Information</h1>
        <p class="page-subtitle">Vigtige oplysninger om projekter, regler, hjælp og aflevering</p>
      </div>

      <!-- Top Category Navigation -->
      <nav class="category-nav" aria-label="Kategorinavigation">
        <button
          v-for="cat in categories"
          :key="cat.id"
          type="button"
          class="category-btn"
          :class="{ active: activeCategory === cat.id }"
          @click="scrollToSection(cat.targetId, cat.id)"
        >
          {{ cat.label }}
        </button>
      </nav>
    </header>

    <!-- Main Content Grid Layout -->
    <div class="info-layout">
      <!-- Left Anchor Sidebar ("På denne side") -->
      <aside class="side-nav-card" aria-label="Indholdsfortegnelse">
        <div class="side-nav-header">PÅ DENNE SIDE</div>
        <nav class="side-nav-list">
          <button
            v-for="sec in pageSections"
            :key="sec.id"
            type="button"
            class="side-nav-item"
            :class="{ active: activeSectionId === sec.id }"
            @click="scrollToSection(sec.id)"
          >
            <component :is="sec.icon" :size="16" :stroke-width="2" class="side-nav-icon" />
            <span>{{ sec.label }}</span>
          </button>
        </nav>

        <div class="side-nav-divider" />

        <!-- Quick contact summary in sidebar -->
        <div class="side-quick-card">
          <div class="side-quick-title">Fraværstelefon</div>
          <p class="side-quick-text">Mellem 07:30–08:00</p>
          <a href="tel:24203441" class="side-quick-phone">
            <IconPhone :size="14" :stroke-width="2" />
            24 20 34 41
          </a>
        </div>
      </aside>

      <!-- Right Main Content Surface -->
      <main class="main-content-surface">
        <!-- Optional Dynamic Opslag from Instructors -->
        <section v-if="infoEntries.length > 0" class="content-block notices-block">
          <div class="block-header">
            <div class="block-title-group">
              <IconPin :size="18" :stroke-width="2.2" class="block-icon text-amber" />
              <h2 class="block-title">Aktuelle opslag fra instruktører</h2>
            </div>
          </div>
          <div class="notices-list">
            <article
              v-for="entry in infoEntries"
              :key="entry.id"
              class="notice-item"
              :class="{ pinned: entry.isPinned }"
            >
              <div class="notice-meta">
                <span v-if="entry.isPinned" class="pinned-pill">Fastgjort</span>
                <span class="notice-date">{{ formatDateTime(entry.createdAt) }}</span>
              </div>
              <h3 class="notice-title">{{ entry.title }}</h3>
              <p class="notice-body">{{ entry.content }}</p>
            </article>
          </div>
        </section>

        <!-- Section 1: Introduktion (Callout Card) -->
        <section id="sec-intro" class="intro-callout">
          <div class="intro-icon-wrap">
            <IconInfoCircle :size="22" :stroke-width="2.2" class="intro-icon" />
          </div>
          <div class="intro-body">
            <h2 class="intro-title">Velkommen til informationssiden</h2>
            <p class="intro-text">
              Her finder du samlet de vigtigste oplysninger om projekter, regler, arbejdsmiljø, support og
              aflevering i SKP OS. Siden er tænkt som dit opslagsværk, så du hurtigt kan finde svar på det, du har brug for i hverdagen.
            </p>
          </div>
        </section>

        <!-- Two-column grid for core content blocks -->
        <div class="sections-grid">
          <!-- Section 2: Projekt & SOP -->
          <section id="sec-projekt" class="content-card">
            <div class="card-header">
              <div class="card-title-group">
                <IconFolder :size="18" :stroke-width="2.2" class="card-icon text-blue" />
                <h2 class="card-title">Projekt & SOP</h2>
              </div>
            </div>
            <p class="card-intro">
              I SKP OS arbejder du med personlige projekter og skabelonbaserede forløb.
            </p>
            <ul class="bullet-list">
              <li>
                <IconCheck :size="15" :stroke-width="2.5" class="bullet-check" />
                <span>Du kan oprette personlige projekter eller vælge et skabelonprojekt.</span>
              </li>
              <li>
                <IconCheck :size="15" :stroke-width="2.5" class="bullet-check" />
                <span>Alle projekter tilknyttes automatisk til din profil.</span>
              </li>
              <li>
                <IconCheck :size="15" :stroke-width="2.5" class="bullet-check" />
                <span>Følg altid din institutions retningslinjer og undervisers anvisninger.</span>
              </li>
              <li>
                <IconCheck :size="15" :stroke-width="2.5" class="bullet-check" />
                <span>Alle projekter og opgaver skal beskrives og dokumenteres i systemet.</span>
              </li>
              <li>
                <IconCheck :size="15" :stroke-width="2.5" class="bullet-check" />
                <span>I arbejdstiden arbejder du aktivt med godkendte opgaver og projekter (37 timer/uge).</span>
              </li>
              <li>
                <IconCheck :size="15" :stroke-width="2.5" class="bullet-check" />
                <span>Fællesopgaver (oprydning i værksted, lager eller lokaler) indgår efter instruks.</span>
              </li>
            </ul>
            <div class="card-footer">
              <button
                type="button"
                class="action-link"
                @click="router.push({ name: 'projekter' })"
              >
                <span>Læs mere om projekttyper og SOP-krav</span>
                <IconArrowRight :size="14" :stroke-width="2" />
              </button>
            </div>
          </section>

          <!-- Section 3: Vigtige regler -->
          <section id="sec-regler" class="content-card">
            <div class="card-header">
              <div class="card-title-group">
                <IconShield :size="18" :stroke-width="2.2" class="card-icon text-slate" />
                <h2 class="card-title">Vigtige regler</h2>
              </div>
            </div>
            <ul class="bullet-list">
              <li>
                <span class="bullet-dot">•</span>
                <span>Overhold altid skolens regler og aftaler.</span>
              </li>
              <li>
                <span class="bullet-dot">•</span>
                <span>Snyd, plagiat og uautoriseret hjælp er ikke tilladt.</span>
              </li>
              <li>
                <span class="bullet-dot">•</span>
                <span>Deadlines skal overholdes medmindre andet er aftalt.</span>
              </li>
              <li>
                <span class="bullet-dot">•</span>
                <span>Kommunikér i god tid hvis du får udfordringer.</span>
              </li>
              <li>
                <span class="bullet-dot">•</span>
                <span>Brug et respektfuldt sprog og vis hensyn i samarbejdet.</span>
              </li>
              <li>
                <span class="bullet-dot">•</span>
                <span>Mødetid er 08:00–15:30 (fredag til 14:30). Sygemelding kl. 07:30–08:00 på tlf. 24 20 34 41 + Ludus.</span>
              </li>
            </ul>
            <div class="card-footer">
              <button
                type="button"
                class="action-link"
                @click="showRulesModal = true"
              >
                <span>Se skolens studie- og ordensreglement for detaljer</span>
                <IconArrowRight :size="14" :stroke-width="2" />
              </button>
            </div>
          </section>

          <!-- Section 4: Kontakt & få hjælp -->
          <section id="sec-kontakt" class="content-card">
            <div class="card-header">
              <div class="card-title-group">
                <IconHeadset :size="18" :stroke-width="2.2" class="card-icon text-blue" />
                <h2 class="card-title">Kontakt & få hjælp</h2>
              </div>
            </div>
            <p class="card-intro">
              Har du spørgsmål eller brug for hjælp, er der flere måder at få fat i os på.
            </p>

            <div class="contact-table-wrap">
              <table class="contact-table" aria-label="Kontaktoversigt">
                <tbody>
                  <tr>
                    <td class="col-role">
                      <div class="role-title">Din underviser</div>
                      <div class="role-desc">Spørgsmål om faglige opgaver og projekter</div>
                    </td>
                    <td class="col-detail">
                      <span class="contact-text">Kontakt via Teams / mail</span>
                      <div class="instructor-pills">
                        <span class="tag" title="Søren (sja@sde.dk)">Søren</span>
                        <span class="tag" title="Zbigniew (zzm@sde.dk)">Zbigniew</span>
                        <span class="tag" title="Kenny (kps@sde.dk)">Kenny</span>
                        <span class="tag" title="John (jcp@sde.dk)">John</span>
                        <span class="tag" title="Karsten (kjef@sde.dk)">Karsten</span>
                        <span class="tag" title="Inge (inab@sde.dk)">Inge</span>
                      </div>
                    </td>
                  </tr>

                  <tr>
                    <td class="col-role">
                      <div class="role-title">IT & support</div>
                      <div class="role-desc">Tekniske problemer og adgang</div>
                    </td>
                    <td class="col-detail">
                      <a href="mailto:helpdesk@itskp-odense.dk" class="contact-link">helpdesk@itskp-odense.dk</a>
                      <div class="sub-link">Portal: <a href="https://support.itskp-odense.dk" target="_blank" rel="noopener">support.itskp-odense.dk</a></div>
                    </td>
                  </tr>

                  <tr>
                    <td class="col-role">
                      <div class="role-title">Studievejledning</div>
                      <div class="role-desc">Studieretning, trivsel og generelle spørgsmål</div>
                    </td>
                    <td class="col-detail">
                      <a href="mailto:bfp@sde.dk" class="contact-link">bfp@sde.dk</a>
                      <div class="sub-link">Brian Floridon Petersen (63 12 66 50)</div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- IT Resources Bar -->
            <div class="tech-resources">
              <div class="tech-item">
                <IconWifi :size="14" class="tech-icon" />
                <span><strong>SOP WiFi:</strong> SKPWiFi (Kode: <code>SKPWire1!</code>)</span>
              </div>
              <div class="tech-item">
                <IconExternalLink :size="14" class="tech-icon" />
                <span><strong>Udlån & serverrum:</strong> <a href="https://support.udlaan.itskp-odense.dk" target="_blank" rel="noopener">support.udlaan.itskp-odense.dk</a></span>
              </div>
            </div>

            <div class="card-footer">
              <button
                type="button"
                class="action-link"
                @click="showRulesModal = true"
              >
                <span>Se kontaktinfo og åbningstider på intranettet</span>
                <IconArrowRight :size="14" :stroke-width="2" />
              </button>
            </div>
          </section>

          <!-- Section 5: Arbejdsmiljø -->
          <section id="sec-arbejdsmiljo" class="content-card">
            <div class="card-header">
              <div class="card-title-group">
                <IconHeart :size="18" :stroke-width="2.2" class="card-icon text-rose" />
                <h2 class="card-title">Arbejdsmiljø</h2>
              </div>
            </div>
            <p class="card-intro">
              Et godt arbejdsmiljø er vigtigt for din hverdag.
            </p>
            <ul class="bullet-list">
              <li>
                <span class="bullet-dot">•</span>
                <span>Sørg for gode pauser og variation i dit arbejde.</span>
              </li>
              <li>
                <span class="bullet-dot">•</span>
                <span>Brug en god arbejdsstilling og skærmpauser.</span>
              </li>
              <li>
                <span class="bullet-dot">•</span>
                <span>Sig til hvis du oplever mistrivsel eller pres.</span>
              </li>
              <li>
                <span class="bullet-dot">•</span>
                <span>Skolen har nulpolitik og klare retningslinjer mod mobning.</span>
              </li>
              <li>
                <span class="bullet-dot">•</span>
                <span><strong>Arbejdsmiljørepræsentant:</strong> Richard Badenszki (Område 9, e-mail: <a href="mailto:62687@edu.sde.dk" class="inline-link">62687@edu.sde.dk</a>).</span>
              </li>
            </ul>
            <div class="card-footer">
              <a href="mailto:62687@edu.sde.dk" class="action-link">
                <span>Læs mere om arbejdsmiljø og trivsel</span>
                <IconArrowRight :size="14" :stroke-width="2" />
              </a>
            </div>
          </section>

          <!-- Section 6: Aflevering -->
          <section id="sec-aflevering" class="content-card">
            <div class="card-header">
              <div class="card-title-group">
                <IconUpload :size="18" :stroke-width="2.2" class="card-icon text-blue" />
                <h2 class="card-title">Aflevering</h2>
              </div>
            </div>
            <p class="card-intro">
              Når du skal aflevere et projekt, skal du følge disse retningslinjer.
            </p>

            <ol class="steps-list">
              <li class="step-item">
                <span class="step-badge">1</span>
                <div class="step-content">
                  <span>Aflever i det rigtige projekt i SKP OS.</span>
                </div>
              </li>
              <li class="step-item">
                <span class="step-badge">2</span>
                <div class="step-content">
                  <span>Upload den korrekte fil (f.eks. ZIP, PDF, DOCX) eller GitHub repository.</span>
                </div>
              </li>
              <li class="step-item">
                <span class="step-badge">3</span>
                <div class="step-content">
                  <span>Tjek at du har vedhæftet alle relevante bilag.</span>
                </div>
              </li>
              <li class="step-item">
                <span class="step-badge">4</span>
                <div class="step-content">
                  <span>Tryk "Aflever" og vent på bekræftelse.</span>
                </div>
              </li>
              <li class="step-item">
                <span class="step-badge">5</span>
                <div class="step-content">
                  <span>Gem kvitteringen for din aflevering.</span>
                </div>
              </li>
            </ol>

            <!-- Important submission alert -->
            <div class="warning-callout">
              <IconAlertTriangle :size="18" :stroke-width="2.2" class="warning-icon" />
              <div class="warning-body">
                <span class="warning-title">Vigtigt</span>
                <p class="warning-text">
                  Aflever kun en endelig version, medmindre andet er aftalt med din underviser.
                </p>
              </div>
            </div>
          </section>

          <!-- Section 7: FAQ – Ofte stillede spørgsmål -->
          <section id="sec-faq" class="content-card">
            <div class="card-header">
              <div class="card-title-group">
                <IconHelpCircle :size="18" :stroke-width="2.2" class="card-icon text-slate" />
                <h2 class="card-title">FAQ – Ofte stillede spørgsmål</h2>
              </div>
            </div>

            <div class="faq-accordion">
              <div
                v-for="(item, idx) in faqs"
                :key="idx"
                class="faq-item"
                :class="{ open: openFaqIndex === idx }"
              >
                <button
                  type="button"
                  class="faq-question-btn"
                  :aria-expanded="openFaqIndex === idx"
                  @click="toggleFaq(idx)"
                >
                  <span class="faq-q-text">{{ item.q }}</span>
                  <IconChevronDown
                    :size="16"
                    :stroke-width="2.2"
                    class="faq-chevron"
                    :class="{ rotated: openFaqIndex === idx }"
                  />
                </button>
                <div v-show="openFaqIndex === idx" class="faq-answer">
                  <p>{{ item.a }}</p>
                </div>
              </div>
            </div>

            <p class="faq-footer-note">
              Kan du ikke finde svaret? Kontakt din underviser eller IT-support.
            </p>
          </section>
        </div>

        <!-- Page Footer Metadata & Feedback Link -->
        <footer class="info-footer">
          <div class="footer-left">
            <IconClock :size="14" :stroke-width="2" class="footer-icon" />
            <span>Sidst opdateret: September 2026</span>
          </div>
          <div class="footer-right">
            <span>Har du forslag til forbedringer?</span>
            <button type="button" class="feedback-link-btn" @click="showFeedbackModal = true">
              <span>Giv feedback</span>
              <IconArrowRight :size="13" :stroke-width="2" />
            </button>
          </div>
        </footer>
      </main>
    </div>

    <!-- Modal: Ordensreglement & Egnethed -->
    <Teleport to="body">
      <div
        v-if="showRulesModal"
        class="modal-overlay"
        role="dialog"
        aria-modal="true"
        aria-labelledby="rules-modal-title"
        @click.self="showRulesModal = false"
      >
        <div class="modal-card">
          <div class="modal-header">
            <div class="modal-title-group">
              <IconShield :size="18" :stroke-width="2.2" class="text-blue" />
              <h3 id="rules-modal-title" class="modal-title">Ordensreglement & Egnethed i SOP</h3>
            </div>
            <button
              type="button"
              class="modal-close"
              aria-label="Luk"
              @click="showRulesModal = false"
            >
              <IconX :size="18" :stroke-width="2" />
            </button>
          </div>

          <div class="modal-content">
            <section class="modal-section">
              <h4>Arbejdstid & fremmøde</h4>
              <p>
                Arbejdstiden er 37 timer pr. uge: mandag til torsdag kl. 08:00–15:30 og fredag kl. 08:00–14:30.
                I arbejdstiden arbejder du koncentreret med godkendte opgaver og projekter i opgavesystemet.
              </p>
            </section>

            <section class="modal-section">
              <h4>Pauser</h4>
              <p>
                • 09:30–10:00 (må ikke forlade området)<br />
                • 11:30–12:00 (frokostpause – må gerne forlade området)<br />
                • 13:30–13:50 (må ikke forlade området)<br />
                Ikke-arbejdsrelaterede aktiviteter må kun finde sted i pauserne.
              </p>
            </section>

            <section class="modal-section">
              <h4>Sygdom & forsinkelse</h4>
              <p>
                Sygemelding skal ske hver dag mellem 07:30–08:00 på <strong>tlf. 24 20 34 41</strong> (gerne SMS) samt registreres i Ludus.
                Forsinkelse skal ligeledes meldes senest kl. 08:00. Ulovligt fravær fører til advarsel i Ludus og træk i FF-tid eller løn.
              </p>
            </section>

            <section class="modal-section">
              <h4>FF-timer & fri</h4>
              <p>
                Du optjener 3 FF-timer pr. måned (maks. 37 timer). Forbrug trækker 7,5 timer mandag–torsdag og 7 timer fredag.
                Fri uden brug af FF gives ved læge/speciallæge, jobsamtale, borgerligt ombud samt begravelse/bryllup i nær familie.
              </p>
            </section>

            <section class="modal-section">
              <h4>EMMA-egnethed</h4>
              <p>
                Du vurderes egnet, når du møder stabilt, forberedt og velsoigneret, overholder aftaler, arbejder aktivt med dine projekter,
                samarbejder positivt og fører en aktiv lærepladssøgning på lærepladsen.dk.
              </p>
            </section>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="showRulesModal = false">
              Luk
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Modal: Feedback & Forslagskasse -->
    <Teleport to="body">
      <div
        v-if="showFeedbackModal"
        class="modal-overlay"
        role="dialog"
        aria-modal="true"
        aria-labelledby="feedback-modal-title"
        @click.self="showFeedbackModal = false"
      >
        <div class="modal-card">
          <div class="modal-header">
            <div class="modal-title-group">
              <IconMessageReport :size="18" :stroke-width="2.2" class="text-blue" />
              <h3 id="feedback-modal-title" class="modal-title">Forslagskasse & feedback</h3>
            </div>
            <button
              type="button"
              class="modal-close"
              aria-label="Luk"
              @click="showFeedbackModal = false"
            >
              <IconX :size="18" :stroke-width="2" />
            </button>
          </div>

          <form class="modal-content" @submit.prevent="submitFeedback">
            <p class="modal-intro">
              Kom med gode idéer, forslag til forbedringer eller rapportér fejl i SKP OS.
              Bemærk: Personlige beskeder til instruktører gives som normalt via mail eller Teams.
            </p>

            <div v-if="feedbackSuccess" class="feedback-success-banner">
              <IconCheck :size="16" :stroke-width="2.5" />
              <span>Tak for dit forslag! Det er nu indsendt til teamet.</span>
            </div>

            <div v-else class="form-fields">
              <div class="form-group">
                <label for="fb-title" class="form-label">Titel <span class="req">*</span></label>
                <input
                  id="fb-title"
                  v-model="feedbackTitle"
                  type="text"
                  class="form-input"
                  placeholder="Kort overskrift på dit forslag..."
                  required
                />
              </div>

              <div class="form-group">
                <label for="fb-desc" class="form-label">Beskrivelse <span class="req">*</span></label>
                <textarea
                  id="fb-desc"
                  v-model="feedbackDesc"
                  class="form-textarea"
                  rows="4"
                  maxlength="1400"
                  placeholder="Beskriv forslaget eller fejlen nærmere (maks 1400 tegn)..."
                  required
                />
                <span class="char-hint">{{ feedbackDesc.length }} / 1400 tegn</span>
              </div>
            </div>

            <div class="modal-footer">
              <button
                type="button"
                class="btn btn-secondary"
                :disabled="isSubmittingFeedback"
                @click="showFeedbackModal = false"
              >
                Annuller
              </button>
              <button
                type="submit"
                class="btn btn-primary"
                :disabled="isSubmittingFeedback || !feedbackTitle.trim() || !feedbackDesc.trim()"
              >
                <IconLoader2 v-if="isSubmittingFeedback" :size="15" class="spinning" />
                <span>{{ isSubmittingFeedback ? "Indsender..." : "Send forslag" }}</span>
              </button>
            </div>
          </form>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
/* Page Layout */
.page-container {
  display: flex;
  flex-direction: column;
  gap: 18px;
  max-width: 1360px;
  margin: 0 auto;
}

/* Page Header */
.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.header-titles {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.page-title {
  font-size: 22px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
  line-height: 1.25;
}

.page-subtitle {
  font-size: 13.5px;
  color: #64748b;
  margin: 0;
  line-height: 1.4;
}

/* Top Category Navigation */
.category-nav {
  display: inline-flex;
  align-items: center;
  background-color: #ffffff;
  border: 1px solid #dde1e5;
  border-radius: 8px;
  padding: 3px;
  gap: 2px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.04);
}

.category-btn {
  display: inline-flex;
  align-items: center;
  padding: 5px 12px;
  border-radius: 6px;
  border: none;
  background: transparent;
  color: #64748b;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  font-family: inherit;
  transition: all 0.15s ease;
  white-space: nowrap;
}

.category-btn:hover {
  color: #0f172a;
  background-color: #f1f5f9;
}

.category-btn.active {
  background-color: #016bff;
  color: #ffffff;
  font-weight: 600;
}

/* 2-Column Info Layout */
.info-layout {
  display: grid;
  grid-template-columns: 210px 1fr;
  gap: 20px;
  align-items: start;
}

/* Left Sidebar Sticky Navigation */
.side-nav-card {
  position: sticky;
  top: 16px;
  background: #ffffff;
  border: 1px solid #dde1e5;
  border-radius: 10px;
  padding: 16px 12px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.side-nav-header {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.05em;
  color: #94a3b8;
  padding: 0 8px 6px;
  text-transform: uppercase;
}

.side-nav-list {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.side-nav-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  border-radius: 6px;
  border: none;
  background: transparent;
  color: #475569;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  font-family: inherit;
  text-align: left;
  transition: all 0.15s ease;
}

.side-nav-item:hover {
  background-color: #f8fafc;
  color: #0f172a;
}

.side-nav-item.active {
  background-color: #eff6ff;
  color: #016bff;
  font-weight: 600;
}

.side-nav-icon {
  color: #64748b;
  flex-shrink: 0;
  transition: color 0.15s;
}

.side-nav-item.active .side-nav-icon {
  color: #016bff;
}

.side-nav-divider {
  height: 1px;
  background-color: #f1f5f9;
  margin: 6px 0;
}

.side-quick-card {
  background-color: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 10px 12px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.side-quick-title {
  font-size: 11px;
  font-weight: 700;
  color: #334155;
}

.side-quick-text {
  font-size: 11px;
  color: #64748b;
  margin: 0;
}

.side-quick-phone {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 12.5px;
  font-weight: 700;
  color: #016bff;
  text-decoration: none;
  margin-top: 4px;
}

.side-quick-phone:hover {
  text-decoration: underline;
}

/* Main Content Surface */
.main-content-surface {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

/* Section 1: Intro Callout */
.intro-callout {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  background-color: #eff6ff;
  border: 1px solid #bfdbfe;
  border-radius: 10px;
  padding: 16px 20px;
}

.intro-icon-wrap {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background-color: #dbeafe;
  color: #016bff;
  flex-shrink: 0;
}

.intro-body {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.intro-title {
  font-size: 15px;
  font-weight: 700;
  color: #1e3a8a;
  margin: 0;
}

.intro-text {
  font-size: 13.5px;
  line-height: 1.55;
  color: #2563eb;
  margin: 0;
}

/* 2-Column Grid for Sections */
.sections-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 18px;
}

/* Common Content Card */
.content-card {
  background: #ffffff;
  border: 1px solid #dde1e5;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  padding: 20px 22px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid #f1f5f9;
  padding-bottom: 10px;
}

.card-title-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.card-icon {
  flex-shrink: 0;
}

.text-blue {
  color: #016bff;
}

.text-slate {
  color: #475569;
}

.text-rose {
  color: #e11d48;
}

.text-amber {
  color: #d97706;
}

.card-title {
  font-size: 15px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.card-intro {
  font-size: 13px;
  line-height: 1.5;
  color: #475569;
  margin: 0;
}

/* Bullet Lists */
.bullet-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.bullet-list li {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  font-size: 13px;
  line-height: 1.5;
  color: #334155;
}

.bullet-check {
  color: #016bff;
  margin-top: 2px;
  flex-shrink: 0;
}

.bullet-dot {
  color: #94a3b8;
  font-weight: 700;
  line-height: 1;
  font-size: 16px;
  flex-shrink: 0;
}

.inline-link {
  color: #016bff;
  text-decoration: none;
  font-weight: 500;
}

.inline-link:hover {
  text-decoration: underline;
}

/* Card Footer with Action Link */
.card-footer {
  margin-top: auto;
  padding-top: 8px;
  border-top: 1px solid #f8fafc;
}

.action-link {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  background: none;
  border: none;
  padding: 0;
  font-size: 12.5px;
  font-weight: 600;
  color: #016bff;
  cursor: pointer;
  font-family: inherit;
  text-decoration: none;
  transition: gap 0.15s ease, color 0.15s ease;
}

.action-link:hover {
  color: #005ae0;
  gap: 8px;
}

/* Contact Table */
.contact-table-wrap {
  width: 100%;
  overflow-x: auto;
}

.contact-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 12.5px;
}

.contact-table tr {
  border-bottom: 1px solid #f1f5f9;
}

.contact-table tr:last-child {
  border-bottom: none;
}

.contact-table td {
  padding: 8px 4px;
  vertical-align: top;
}

.col-role {
  width: 44%;
}

.role-title {
  font-weight: 600;
  color: #0f172a;
}

.role-desc {
  font-size: 11px;
  color: #64748b;
  margin-top: 1px;
}

.col-detail {
  width: 56%;
}

.contact-text {
  color: #475569;
  display: block;
}

.contact-link {
  color: #016bff;
  text-decoration: none;
  font-weight: 600;
}

.contact-link:hover {
  text-decoration: underline;
}

.sub-link {
  font-size: 11.5px;
  color: #64748b;
  margin-top: 2px;
}

.sub-link a {
  color: #016bff;
  text-decoration: none;
}

.sub-link a:hover {
  text-decoration: underline;
}

.instructor-pills {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  margin-top: 4px;
}

.tag {
  display: inline-block;
  padding: 1px 6px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 500;
  background-color: #f1f5f9;
  border: 1px solid #e2e8f0;
  color: #334155;
}

/* Tech Resources Bar */
.tech-resources {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 8px 12px;
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 12px;
  color: #334155;
  margin-top: 4px;
}

.tech-item {
  display: flex;
  align-items: center;
  gap: 6px;
}

.tech-icon {
  color: #64748b;
  flex-shrink: 0;
}

.tech-item code {
  background-color: #edf2f7;
  padding: 1px 4px;
  border-radius: 3px;
  font-family: inherit;
  font-size: 11.5px;
  color: #0f172a;
}

.tech-item a {
  color: #016bff;
  text-decoration: none;
}

.tech-item a:hover {
  text-decoration: underline;
}

/* Step-by-Step Submission Process */
.steps-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.step-item {
  display: flex;
  align-items: flex-start;
  gap: 10px;
}

.step-badge {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background-color: #016bff;
  color: #ffffff;
  font-size: 11px;
  font-weight: 700;
  flex-shrink: 0;
  margin-top: 1px;
}

.step-content {
  display: flex;
  flex-direction: column;
  gap: 1px;
  font-size: 12.5px;
  line-height: 1.45;
  color: #334155;
}

.step-content strong {
  color: #0f172a;
}

/* Warning Callout */
.warning-callout {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  background-color: #fffbeb;
  border: 1px solid #fef3c7;
  border-left: 3px solid #f59e0b;
  border-radius: 6px;
  padding: 10px 12px;
  margin-top: 4px;
}

.warning-icon {
  color: #d97706;
  flex-shrink: 0;
  margin-top: 1px;
}

.warning-body {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.warning-title {
  font-size: 12px;
  font-weight: 700;
  color: #92400e;
}

.warning-text {
  font-size: 12px;
  line-height: 1.45;
  color: #b45309;
  margin: 0;
}

/* FAQ Accordion */
.faq-accordion {
  display: flex;
  flex-direction: column;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  overflow: hidden;
}

.faq-item {
  border-bottom: 1px solid #e2e8f0;
}

.faq-item:last-child {
  border-bottom: none;
}

.faq-question-btn {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 10px 14px;
  background: #ffffff;
  border: none;
  text-align: left;
  cursor: pointer;
  font-family: inherit;
  transition: background-color 0.15s ease;
}

.faq-question-btn:hover {
  background-color: #f8fafc;
}

.faq-item.open .faq-question-btn {
  background-color: #f8fafc;
}

.faq-q-text {
  font-size: 12.5px;
  font-weight: 600;
  color: #0f172a;
  line-height: 1.4;
}

.faq-chevron {
  color: #94a3b8;
  flex-shrink: 0;
  transition: transform 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.faq-chevron.rotated {
  transform: rotate(180deg);
}

.faq-answer {
  padding: 10px 14px 14px;
  background-color: #ffffff;
  font-size: 12.5px;
  line-height: 1.55;
  color: #475569;
}

.faq-answer p {
  margin: 0;
}

.faq-footer-note {
  font-size: 12px;
  color: #64748b;
  margin: 4px 0 0;
  line-height: 1.4;
}

/* Notice Block (Dynamic opslag) */
.notices-block {
  background: #ffffff;
  border: 1px solid #fef3c7;
  border-radius: 10px;
  padding: 16px 20px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

.block-header {
  margin-bottom: 12px;
}

.block-title-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.block-title {
  font-size: 14.5px;
  font-weight: 700;
  color: #92400e;
  margin: 0;
}

.notices-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.notice-item {
  padding: 10px 12px;
  border-radius: 6px;
  background-color: #fffdf5;
  border: 1px solid #fde68a;
}

.notice-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 3px;
}

.pinned-pill {
  font-size: 10.5px;
  font-weight: 700;
  color: #b45309;
  background-color: #fef3c7;
  padding: 1px 6px;
  border-radius: 999px;
}

.notice-date {
  font-size: 11px;
  color: #78716c;
}

.notice-title {
  font-size: 13.5px;
  font-weight: 600;
  color: #1c1917;
  margin: 0 0 2px;
}

.notice-body {
  font-size: 12.5px;
  line-height: 1.5;
  color: #44403c;
  margin: 0;
  white-space: pre-wrap;
}

/* Footer Section */
.info-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 12px;
  padding: 14px 20px;
  background: #ffffff;
  border: 1px solid #dde1e5;
  border-radius: 10px;
  font-size: 12.5px;
  color: #64748b;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.footer-left {
  display: flex;
  align-items: center;
  gap: 6px;
}

.footer-icon {
  color: #94a3b8;
}

.footer-right {
  display: flex;
  align-items: center;
  gap: 6px;
}

.feedback-link-btn {
  background: none;
  border: none;
  padding: 0;
  color: #016bff;
  font-size: 12.5px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  text-decoration: none;
}

.feedback-link-btn:hover {
  text-decoration: underline;
}

/* Modals */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.5);
  backdrop-filter: blur(2px);
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.modal-card {
  background: #ffffff;
  border-radius: 12px;
  width: 100%;
  max-width: 560px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
  overflow: hidden;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid #e2e8f0;
}

.modal-title-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.modal-title {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.modal-close {
  background: none;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 4px;
  border-radius: 6px;
  transition: all 0.15s;
}

.modal-close:hover {
  color: #0f172a;
  background-color: #f1f5f9;
}

.modal-content {
  padding: 20px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.modal-section h4 {
  font-size: 13.5px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 4px;
}

.modal-section p {
  font-size: 12.5px;
  line-height: 1.55;
  color: #475569;
  margin: 0;
}

.modal-intro {
  font-size: 13px;
  line-height: 1.5;
  color: #475569;
  margin: 0;
}

.form-fields {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.form-label {
  font-size: 12.5px;
  font-weight: 600;
  color: #334155;
}

.form-label .req {
  color: #e11d48;
}

.form-input,
.form-textarea {
  width: 100%;
  padding: 8px 10px;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  font-family: inherit;
  font-size: 13px;
  color: #0f172a;
  outline: none;
  box-sizing: border-box;
  transition: border-color 0.15s ease;
}

.form-input:focus,
.form-textarea:focus {
  border-color: #016bff;
}

.char-hint {
  font-size: 11px;
  color: #94a3b8;
  text-align: right;
}

.feedback-success-banner {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: #f0fdf4;
  border: 1px solid #bbf7d0;
  color: #15803d;
  padding: 12px 14px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
}

.modal-footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
  padding: 14px 20px;
  border-top: 1px solid #e2e8f0;
  background-color: #f8fafc;
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  height: 34px;
  padding: 0 14px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  font-family: inherit;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-primary {
  background-color: #016bff;
  color: #ffffff;
  border: 1px solid #016bff;
}

.btn-primary:hover:not(:disabled) {
  background-color: #005ae0;
}

.btn-primary:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: #ffffff;
  color: #475569;
  border: 1px solid #cbd5e1;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #f1f5f9;
  color: #0f172a;
}

.spinning {
  animation: spin 0.9s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

/* Responsive Behavior */
@media (max-width: 1080px) {
  .info-layout {
    grid-template-columns: 190px 1fr;
    gap: 16px;
  }

  .sections-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 900px) {
  .info-layout {
    grid-template-columns: 1fr;
  }

  .side-nav-card {
    position: static;
    flex-direction: row;
    overflow-x: auto;
    padding: 8px 10px;
  }

  .side-nav-header,
  .side-nav-divider,
  .side-quick-card {
    display: none;
  }

  .side-nav-list {
    flex-direction: row;
    flex-wrap: nowrap;
  }

  .side-nav-item {
    white-space: nowrap;
    padding: 6px 10px;
    font-size: 12px;
  }

  .category-nav {
    overflow-x: auto;
    max-width: 100%;
  }
}
</style>