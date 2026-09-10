<script setup lang="ts">
import { ref, computed, onMounted, watch, watchEffect } from "vue";
import {
  IconInfoCircle,
  IconChevronDown,
  IconChevronLeft,
  IconChevronRight,
  IconSearch,
} from "@tabler/icons-vue";
import { useFFEntryStore } from "@/Stores/FFEntryStore";
import { useStudentProfileStore } from "@/Stores/StudentProfileStore";
import { api } from "@/Services/api";
import type { FFEntryDto, InstructorFreeEntryDto } from "@/types";

const ffEntryStore = useFFEntryStore();
const studentProfileStore = useStudentProfileStore();

const isLoading = ref(true);

const INFO_EXPANDED_KEY = "ff_info_expanded";
const infoExpanded = ref(localStorage.getItem(INFO_EXPANDED_KEY) === "true");
watchEffect(() => {
  localStorage.setItem(INFO_EXPANDED_KEY, String(infoExpanded.value));
});

const searchQuery1 = ref("");
const rowsPerPage1 = ref(10);
const currentPage1 = ref(1);

const instructorFreeEntries = ref<InstructorFreeEntryDto[]>([]);
const searchQuery2 = ref("");
const rowsPerPage2 = ref(10);
const currentPage2 = ref(1);

watch(searchQuery1, () => {
  currentPage1.value = 1;
});
watch(rowsPerPage1, () => {
  currentPage1.value = 1;
});

watch(searchQuery2, () => {
  currentPage2.value = 1;
});
watch(rowsPerPage2, () => {
  currentPage2.value = 1;
});

function parseDurationToMinutes(durationStr?: string | null): number {
  if (!durationStr) return 0;
  const str = durationStr.trim();
  const isNegative = str.startsWith("-");
  const clean = isNegative ? str.substring(1).trim() : str;

  const tMinMatch = clean.match(/^(\d+)t\s*(\d+)?(?:min)?$/i);
  if (tMinMatch) {
    const hours = parseInt(tMinMatch[1], 10) || 0;
    const minutes = parseInt(tMinMatch[2], 10) || 0;
    const total = hours * 60 + minutes;
    return isNegative ? -total : total;
  }

  let days = 0;
  let timePart = clean;
  if (clean.includes(".")) {
    const parts = clean.split(".");
    days = parseInt(parts[0], 10) || 0;
    timePart = parts[1] || "";
  }

  const pieces = timePart.split(":");
  const hours = parseInt(pieces[0], 10) || 0;
  const minutes = parseInt(pieces[1], 10) || 0;
  const total = days * 24 * 60 + hours * 60 + minutes;
  return isNegative ? -total : total;
}

function formatBalance(minutes: number): string {
  const isNegative = minutes < 0;
  const abs = Math.abs(minutes);
  const h = Math.floor(abs / 60);
  const m = abs % 60;
  const hLabel = h === 1 ? "time" : "timer";
  const mLabel = m === 1 ? "minut" : "minutter";
  return `${isNegative ? "-" : ""}${h} ${hLabel} og ${m} ${mLabel}`;
}

const totalBalanceMinutes = computed(() => {
  return ffEntryStore.FF_ENTRIES.reduce((acc, entry) => {
    return acc + parseDurationToMinutes(entry.duration);
  }, 0);
});

const formattedBalance = computed(() => {
  return formatBalance(totalBalanceMinutes.value);
});

const balanceStatusClass = computed(() => {
  if (totalBalanceMinutes.value < 0) return "negative";
  if (totalBalanceMinutes.value > 37 * 60) return "warning";
  return "normal";
});

function formatDate(dateStr?: string | null): string {
  if (!dateStr) return "";
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  const day = String(d.getDate()).padStart(2, "0");
  const month = String(d.getMonth() + 1).padStart(2, "0");
  const year = d.getFullYear();
  return `${day}-${month}-${year}`;
}

function formatDateTime(dateStr?: string | null): string {
  if (!dateStr) return "";
  if (dateStr.includes(" -> ") || dateStr.includes(" ")) {
    return dateStr;
  }
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  const day = String(d.getDate()).padStart(2, "0");
  const month = String(d.getMonth() + 1).padStart(2, "0");
  const year = d.getFullYear();
  const hours = String(d.getHours()).padStart(2, "0");
  const mins = String(d.getMinutes()).padStart(2, "0");
  return `${day}-${month}-${year} ${hours}:${mins}`;
}

function formatCreatedCell(entry: FFEntryDto): string {
  const dateFormatted = formatDate(entry.date);
  if (entry.createdBy) {
    return `${dateFormatted} / ${entry.createdBy}`;
  }
  if (entry.note && entry.note.toLowerCase().includes("justering")) {
    return `${dateFormatted} / SKP`;
  }
  return dateFormatted;
}

function getEarned(entry: FFEntryDto): string | null {
  const minutes = parseDurationToMinutes(entry.duration);
  if (minutes > 0) {
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return `${h}t ${String(m).padStart(2, "0")}min`;
  }
  return null;
}

function getUsed(entry: FFEntryDto): string | null {
  const minutes = parseDurationToMinutes(entry.duration);
  if (minutes < 0) {
    const abs = Math.abs(minutes);
    const h = Math.floor(abs / 60);
    const m = abs % 60;
    return `-${h}t ${String(m).padStart(2, "0")}min`;
  }
  return null;
}

const filteredFFEntries = computed(() => {
  const q = searchQuery1.value.trim().toLowerCase();
  const sorted = [...ffEntryStore.FF_ENTRIES].sort(
    (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime(),
  );
  if (!q) return sorted;
  return sorted.filter((e) => {
    const created = formatCreatedCell(e).toLowerCase();
    const earned = (getEarned(e) || "").toLowerCase();
    const used = (getUsed(e) || "").toLowerCase();
    const from = (formatDateTime(e.validFrom) || "").toLowerCase();
    const to = (formatDateTime(e.validTo) || "").toLowerCase();
    const note = (e.note || "").toLowerCase();
    return (
      created.includes(q) ||
      earned.includes(q) ||
      used.includes(q) ||
      from.includes(q) ||
      to.includes(q) ||
      note.includes(q)
    );
  });
});

const totalRows1 = computed(() => filteredFFEntries.value.length);
const totalPages1 = computed(() =>
  Math.max(1, Math.ceil(totalRows1.value / rowsPerPage1.value)),
);

const pagedFFEntries = computed(() => {
  const start = (currentPage1.value - 1) * rowsPerPage1.value;
  return filteredFFEntries.value.slice(start, start + rowsPerPage1.value);
});

const pageStart1 = computed(() =>
  totalRows1.value === 0
    ? 0
    : (currentPage1.value - 1) * rowsPerPage1.value + 1,
);
const pageEnd1 = computed(() =>
  Math.min(currentPage1.value * rowsPerPage1.value, totalRows1.value),
);

const visiblePages1 = computed(() => {
  const total = totalPages1.value;
  const cur = currentPage1.value;
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
  const pages: (number | "...")[] = [1];
  if (cur > 3) pages.push("...");
  for (let p = Math.max(2, cur - 1); p <= Math.min(total - 1, cur + 1); p++)
    pages.push(p);
  if (cur < total - 2) pages.push("...");
  pages.push(total);
  return pages;
});

function goToPage1(p: number | "...") {
  if (typeof p === "number") currentPage1.value = p;
}
function prevPage1() {
  if (currentPage1.value > 1) currentPage1.value--;
}
function nextPage1() {
  if (currentPage1.value < totalPages1.value) currentPage1.value++;
}
function resetFilter1() {
  searchQuery1.value = "";
  currentPage1.value = 1;
}

const filteredInstructorFreeEntries = computed(() => {
  const q = searchQuery2.value.trim().toLowerCase();
  const list = [...instructorFreeEntries.value];
  if (!q) return list;
  return list.filter((e) => {
    const created = (
      (e.createdDate || formatDate(e.date)) +
      (e.createdBy ? ` / ${e.createdBy}` : "")
    ).toLowerCase();
    const period = (e.period || "").toLowerCase();
    const duration = (e.duration || "").toLowerCase();
    const category = (e.category || "").toLowerCase();
    const note = (e.note || "").toLowerCase();
    return (
      created.includes(q) ||
      period.includes(q) ||
      duration.includes(q) ||
      category.includes(q) ||
      note.includes(q)
    );
  });
});

const totalRows2 = computed(() => filteredInstructorFreeEntries.value.length);
const totalPages2 = computed(() =>
  Math.max(1, Math.ceil(totalRows2.value / rowsPerPage2.value)),
);

const pagedInstructorFreeEntries = computed(() => {
  const start = (currentPage2.value - 1) * rowsPerPage2.value;
  return filteredInstructorFreeEntries.value.slice(
    start,
    start + rowsPerPage2.value,
  );
});

const pageStart2 = computed(() =>
  totalRows2.value === 0
    ? 0
    : (currentPage2.value - 1) * rowsPerPage2.value + 1,
);
const pageEnd2 = computed(() =>
  Math.min(currentPage2.value * rowsPerPage2.value, totalRows2.value),
);

const visiblePages2 = computed(() => {
  const total = totalPages2.value;
  const cur = currentPage2.value;
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
  const pages: (number | "...")[] = [1];
  if (cur > 3) pages.push("...");
  for (let p = Math.max(2, cur - 1); p <= Math.min(total - 1, cur + 1); p++)
    pages.push(p);
  if (cur < total - 2) pages.push("...");
  pages.push(total);
  return pages;
});

function goToPage2(p: number | "...") {
  if (typeof p === "number") currentPage2.value = p;
}
function prevPage2() {
  if (currentPage2.value > 1) currentPage2.value--;
}
function nextPage2() {
  if (currentPage2.value < totalPages2.value) currentPage2.value++;
}
function resetFilter2() {
  searchQuery2.value = "";
  currentPage2.value = 1;
}

onMounted(async () => {
  try {
    const profile = await studentProfileStore.GET_MY_STUDENT_PROFILE();
    await ffEntryStore.GET_FF_ENTRIES(profile?.id);

    try {
      const response = await api({
        url: "/api/instructorfree",
        method: "GET",
        params: { studentProfileId: profile?.id },
      });
      if (response.data && Array.isArray(response.data)) {
        instructorFreeEntries.value = response.data;
      }
    } catch {
      instructorFreeEntries.value = [];
    }
  } catch {
    await ffEntryStore.GET_FF_ENTRIES();
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h1 class="page-title">FF Timer</h1>
    </div>

    <template v-if="isLoading">
      <div class="skeleton-surface" />
    </template>

    <template v-else>
      <div class="surface">
        <section class="section section-info">
          <button
            class="info-toggle"
            :aria-expanded="infoExpanded"
            @click="infoExpanded = !infoExpanded"
          >
            <span class="info-toggle-left">
              <IconInfoCircle :size="13" :stroke-width="2.2" />
              <span>Vejledning</span>
            </span>
            <IconChevronDown
              :size="14"
              :stroke-width="2.2"
              class="info-chevron"
              :class="{ expanded: infoExpanded }"
            />
          </button>

          <Transition name="info-collapse">
            <div v-if="infoExpanded" class="info-content">
              <p class="info-body">
                Din arbejdsuge er på 37 timer med normal mødetid mellem
                8.00–15.30 (fredag til 14.30).
              </p>
              <ul class="info-list">
                <li>Hver måned opspares 3 timer automatisk (FF-timer).</li>
                <li>Plus-timer videreføres ikke (kan ikke udbetales).</li>
                <li>Minus-timer er ikke tilladt.</li>
                <li>Maksimalt 37 FF-timer på din konto.</li>
              </ul>
              <p class="info-note">
                <strong>OBS:</strong> Der må maksimalt stå 37 FF-timer på din
                konto. Har du mere end dette, skal du planlægge hvordan du
                afvikler FF-timer, så du kommer under denne grænse.
              </p>
            </div>
          </Transition>
        </section>

        <div class="divider" />

        <section class="section section-balance">
          <div class="balance-container">
            <span class="balance-label">Din FF-saldo</span>
            <div class="balance-value-row">
              <span class="balance-text">{{ formattedBalance }}</span>
              <span
                v-if="totalBalanceMinutes > 37 * 60"
                class="balance-badge warning"
              >
                Over grænsen (maks 37t)
              </span>
              <span
                v-else-if="totalBalanceMinutes < 0"
                class="balance-badge negative"
              >
                Minus-timer ikke tilladt
              </span>
            </div>
          </div>
        </section>

        <div class="divider" />

        <section class="section section-table">
          <div class="section-heading">Afholdte og optjente FF-timer</div>

          <div class="table-toolbar">
            <div class="rows-select-group">
              <label for="ff-rows-per-page" class="toolbar-label">Vis</label>
              <select
                id="ff-rows-per-page"
                v-model="rowsPerPage1"
                class="rows-select"
              >
                <option :value="10">10</option>
                <option :value="25">25</option>
                <option :value="50">50</option>
              </select>
              <span class="toolbar-label">rækker pr. side</span>
            </div>

            <div class="toolbar-right">
              <button
                v-if="searchQuery1.trim().length > 0"
                class="reset-btn"
                type="button"
                @click="resetFilter1"
              >
                Nulstil
              </button>
              <div class="search-wrapper">
                <IconSearch :size="14" :stroke-width="2" class="search-icon" />
                <input
                  id="ff-search"
                  v-model="searchQuery1"
                  type="text"
                  class="search-input"
                  placeholder="Søg…"
                  aria-label="Søg i FF-timer"
                />
              </div>
            </div>
          </div>

          <div class="table-responsive">
            <table
              class="data-table"
              aria-label="Afholdte og optjente FF-timer"
            >
              <thead>
                <tr>
                  <th class="col-created">Oprettet dato/af</th>
                  <th class="col-earned">Optjent</th>
                  <th class="col-used">Afviklet</th>
                  <th class="col-valid-from">Gyldig fra</th>
                  <th class="col-valid-to">Gyldig til</th>
                  <th class="col-note">Notat</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="pagedFFEntries.length === 0">
                  <td colspan="6" class="empty-row">
                    Ingen FF-registreringer fundet.
                  </td>
                </tr>
                <tr
                  v-for="entry in pagedFFEntries"
                  :key="entry.id"
                  class="table-row"
                >
                  <td class="cell-created">
                    {{ formatCreatedCell(entry) }}
                  </td>
                  <td class="cell-earned">
                    <span v-if="getEarned(entry)" class="duration-earned">
                      {{ getEarned(entry) }}
                    </span>
                    <span v-else class="cell-muted">—</span>
                  </td>
                  <td class="cell-used">
                    <span v-if="getUsed(entry)" class="duration-used">
                      {{ getUsed(entry) }}
                    </span>
                    <span v-else class="cell-muted">—</span>
                  </td>
                  <td class="cell-date-time">
                    {{ formatDateTime(entry.validFrom) || "—" }}
                  </td>
                  <td class="cell-date-time">
                    {{ formatDateTime(entry.validTo) || "—" }}
                  </td>
                  <td class="cell-note" :title="entry.note">
                    {{ entry.note || "—" }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="table-footer">
            <span class="rows-count">
              Viser {{ pageStart1 }} til {{ pageEnd1 }} af
              {{ totalRows1 }} rækker
            </span>

            <nav class="pagination" aria-label="Pagination FF-timer">
              <button
                class="page-btn"
                :disabled="currentPage1 === 1"
                @click="prevPage1"
                aria-label="Forrige side"
              >
                <IconChevronLeft :size="14" :stroke-width="2.2" />
              </button>
              <template v-for="(p, i) in visiblePages1" :key="i">
                <span v-if="p === '...'" class="page-ellipsis">…</span>
                <button
                  v-else
                  class="page-btn"
                  :class="{ active: p === currentPage1 }"
                  @click="goToPage1(p)"
                  :aria-current="p === currentPage1 ? 'page' : undefined"
                >
                  {{ p }}
                </button>
              </template>
              <button
                class="page-btn"
                :disabled="currentPage1 === totalPages1"
                @click="nextPage1"
                aria-label="Næste side"
              >
                <IconChevronRight :size="14" :stroke-width="2.2" />
              </button>
            </nav>
          </div>
        </section>

        <div class="divider" />

        <section class="section section-table">
          <div class="section-header-block">
            <div class="section-heading">Fri registreret af din instruktør</div>
            <p class="section-subtext">
              Her kan du se fri registreret af din instruktør, f.eks. lægebesøg,
              sygehus eller anden aftalt fri.
            </p>
          </div>

          <div class="table-toolbar">
            <div class="rows-select-group">
              <label for="instructor-rows-per-page" class="toolbar-label"
                >Vis</label
              >
              <select
                id="instructor-rows-per-page"
                v-model="rowsPerPage2"
                class="rows-select"
              >
                <option :value="10">10</option>
                <option :value="25">25</option>
                <option :value="50">50</option>
              </select>
              <span class="toolbar-label">rækker pr. side</span>
            </div>

            <div class="toolbar-right">
              <button
                v-if="searchQuery2.trim().length > 0"
                class="reset-btn"
                type="button"
                @click="resetFilter2"
              >
                Nulstil
              </button>
              <div class="search-wrapper">
                <IconSearch :size="14" :stroke-width="2" class="search-icon" />
                <input
                  id="instructor-search"
                  v-model="searchQuery2"
                  type="text"
                  class="search-input"
                  placeholder="Søg…"
                  aria-label="Søg i fri registreret"
                />
              </div>
            </div>
          </div>

          <div class="table-responsive">
            <table
              class="data-table"
              aria-label="Fri registreret af din instruktør"
            >
              <thead>
                <tr>
                  <th class="col-instructor-created">Oprettet dato/af</th>
                  <th class="col-period">Periode</th>
                  <th class="col-duration">Varighed</th>
                  <th class="col-category">Kategori</th>
                  <th class="col-instructor-note">Notat</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="pagedInstructorFreeEntries.length === 0">
                  <td colspan="5" class="empty-row">
                    Ingen fri registreret fundet.
                  </td>
                </tr>
                <tr
                  v-for="entry in pagedInstructorFreeEntries"
                  :key="entry.id"
                  class="table-row"
                >
                  <td class="cell-created">
                    {{ entry.createdDate || formatDate(entry.date) }}
                    <template v-if="entry.createdBy">
                      / {{ entry.createdBy }}
                    </template>
                  </td>
                  <td class="cell-period">
                    {{ entry.period }}
                  </td>
                  <td class="cell-duration">
                    <span class="duration-neutral">{{ entry.duration }}</span>
                  </td>
                  <td class="cell-category">
                    <span class="category-chip">{{ entry.category }}</span>
                  </td>
                  <td class="cell-note" :title="entry.note">
                    {{ entry.note || "—" }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="table-footer">
            <span class="rows-count">
              Viser {{ pageStart2 }} til {{ pageEnd2 }} af
              {{ totalRows2 }} rækker
            </span>

            <nav class="pagination" aria-label="Pagination fri registreret">
              <button
                class="page-btn"
                :disabled="currentPage2 === 1"
                @click="prevPage2"
                aria-label="Forrige side"
              >
                <IconChevronLeft :size="14" :stroke-width="2.2" />
              </button>
              <template v-for="(p, i) in visiblePages2" :key="i">
                <span v-if="p === '...'" class="page-ellipsis">…</span>
                <button
                  v-else
                  class="page-btn"
                  :class="{ active: p === currentPage2 }"
                  @click="goToPage2(p)"
                  :aria-current="p === currentPage2 ? 'page' : undefined"
                >
                  {{ p }}
                </button>
              </template>
              <button
                class="page-btn"
                :disabled="currentPage2 === totalPages2"
                @click="nextPage2"
                aria-label="Næste side"
              >
                <IconChevronRight :size="14" :stroke-width="2.2" />
              </button>
            </nav>
          </div>
        </section>
      </div>
    </template>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.page-header {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.page-title {
  font-size: 24px;
  font-weight: 700;
  color: #111827;
}

.surface {
  background: #ffffff;
  border: 1px solid #dde1e5;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
  overflow: hidden;
}

.skeleton-surface {
  height: 640px;
  background: linear-gradient(90deg, #e5eaed 25%, #eff2f4 50%, #e5eaed 75%);
  background-size: 400% 100%;
  animation: shimmer 1.4s ease infinite;
  border-radius: 10px;
}

@keyframes shimmer {
  0% {
    background-position: 100% 0;
  }
  100% {
    background-position: -100% 0;
  }
}

.section {
  padding: 18px 22px;
}

.section-info {
  background: #f8fafc;
}

.info-toggle {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: none;
  border: none;
  padding: 0;
  cursor: pointer;
  font-family: inherit;
  outline: none;
}

.info-toggle:focus-visible {
  outline: 2px solid #016bff;
  outline-offset: 2px;
  border-radius: 4px;
}

.info-toggle-left {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 11px;
  font-weight: 700;
  color: #6b7280;
}

.info-chevron {
  color: #9ca3af;
  transition: transform 0.25s cubic-bezier(0.4, 0, 0.2, 1);
}

.info-chevron.expanded {
  transform: rotate(180deg);
}

.info-content {
  overflow: hidden;
}

.info-body {
  font-size: 13px;
  color: #4b5563;
  line-height: 1.6;
  margin-top: 10px;
  margin-bottom: 10px;
  max-width: 72ch;
}

.info-list {
  margin: 0 0 10px 18px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.info-list li {
  font-size: 12.5px;
  color: #4b5563;
  line-height: 1.5;
}

.info-note {
  font-size: 12px;
  color: #6b7280;
  line-height: 1.5;
}

.divider {
  height: 1px;
  background: #dde1e5;
}

.section-balance {
  padding: 14px 22px;
  background: #ffffff;
}

.balance-container {
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.balance-label {
  font-size: 12px;
  font-weight: 700;
  color: #6b7280;
}

.balance-value-row {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.balance-text {
  font-size: 14px;
  font-weight: 700;
  color: #111827;
  letter-spacing: -0.2px;
}

.balance-badge {
  font-size: 11px;
  font-weight: 600;
  padding: 2px 7px;
  border-radius: 4px;
}

.balance-badge.warning {
  background: #fef3c7;
  color: #b45309;
  border: 1px solid #fde68a;
}

.balance-badge.negative {
  background: #fee2e2;
  color: #b91c1c;
  border: 1px solid #fecaca;
}

.section-table {
  padding-top: 20px;
  padding-bottom: 22px;
}

.section-header-block {
  margin-bottom: 12px;
}

.section-heading {
  font-size: 12px;
  font-weight: 700;
  color: #6b7280;
  margin-bottom: 12px;
}

.section-subtext {
  font-size: 12.5px;
  color: #6b7280;
  line-height: 1.5;
  margin-top: -6px;
  margin-bottom: 12px;
}

.table-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 10px;
  margin-bottom: 12px;
}

.rows-select-group {
  display: flex;
  align-items: center;
  gap: 7px;
}

.toolbar-label {
  font-size: 12.5px;
  font-weight: 500;
  color: #6b7280;
}

.rows-select {
  height: 30px;
  padding: 0 8px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-family: inherit;
  font-size: 12.5px;
  font-weight: 600;
  color: #111827;
  outline: none;
  cursor: pointer;
  transition: border-color 0.2s;
}

.rows-select:focus {
  border-color: #016bff;
}

.toolbar-right {
  display: flex;
  align-items: center;
  gap: 8px;
}

.reset-btn {
  display: inline-flex;
  align-items: center;
  height: 30px;
  padding: 0 10px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-family: inherit;
  font-size: 12px;
  font-weight: 600;
  color: #6b7280;
  cursor: pointer;
  outline: none;
  transition:
    border-color 0.15s,
    color 0.15s,
    background-color 0.15s;
}

.reset-btn:hover {
  border-color: #016bff;
  color: #016bff;
  background: #eff6ff;
}

.reset-btn:active {
  transform: scale(0.97);
}

.search-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: 9px;
  color: #9ca3af;
  pointer-events: none;
  transition: color 0.2s;
}

.search-input {
  height: 30px;
  width: 180px;
  padding: 0 10px 0 28px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-family: inherit;
  font-size: 12.5px;
  color: #111827;
  outline: none;
  transition:
    border-color 0.2s,
    background-color 0.2s;
}

.search-input::placeholder {
  color: #9ca3af;
}

.search-input:focus {
  border-color: #016bff;
  background: #ffffff;
}

.search-wrapper:focus-within .search-icon {
  color: #016bff;
}

.table-responsive {
  width: 100%;
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.data-table thead tr {
  border-bottom: 1px solid #e5e7eb;
}

.data-table th {
  padding: 7px 12px;
  font-size: 11px;
  font-weight: 700;
  color: #9ca3af;
  text-align: left;
  white-space: nowrap;
}

.col-created {
  width: 190px;
}
.col-earned {
  width: 110px;
}
.col-used {
  width: 110px;
}
.col-valid-from {
  width: 150px;
}
.col-valid-to {
  width: 150px;
}
.col-note {
  min-width: 160px;
}

.col-instructor-created {
  width: 190px;
}
.col-period {
  width: 250px;
}
.col-duration {
  width: 110px;
}
.col-category {
  width: 180px;
}
.col-instructor-note {
  min-width: 160px;
}

.table-row {
  border-bottom: 1px solid #f3f4f6;
  transition: background-color 0.12s;
}

.table-row:last-child {
  border-bottom: none;
}

.table-row:hover {
  background: #f9fafb;
}

.data-table td {
  padding: 9px 12px;
  vertical-align: middle;
}

.cell-created {
  font-size: 12.5px;
  font-weight: 600;
  color: #374151;
  white-space: nowrap;
}

.cell-earned,
.cell-used {
  font-size: 12.5px;
  white-space: nowrap;
}

.duration-earned {
  font-weight: 600;
  color: #15803d;
}

.duration-used {
  font-weight: 600;
  color: #374151;
}

.duration-neutral {
  font-weight: 600;
  color: #374151;
  font-size: 12.5px;
}

.cell-date-time {
  font-size: 12.5px;
  color: #4b5563;
  white-space: nowrap;
}

.cell-period {
  font-size: 12.5px;
  color: #374151;
  white-space: nowrap;
}

.cell-category {
  white-space: nowrap;
}

.category-chip {
  display: inline-block;
  font-size: 11.5px;
  font-weight: 600;
  color: #374151;
  background: #f1f5f9;
  border: 1px solid #e2e8f0;
  border-radius: 4px;
  padding: 2px 8px;
}

.cell-muted {
  color: #9ca3af;
}

.cell-note {
  font-size: 13px;
  color: #4b5563;
  line-height: 1.45;
  max-width: 280px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.empty-row {
  text-align: center;
  color: #9ca3af;
  font-size: 12.5px;
  padding: 28px 0;
}

.table-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 12px;
}

.rows-count {
  font-size: 12px;
  color: #9ca3af;
  font-weight: 500;
}

.pagination {
  display: flex;
  align-items: center;
  gap: 3px;
}

.page-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 30px;
  height: 30px;
  padding: 0 5px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  background: #ffffff;
  color: #374151;
  font-size: 12.5px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  outline: none;
  transition:
    border-color 0.15s,
    background-color 0.15s,
    color 0.15s,
    transform 0.1s;
}

.page-btn:hover:not(:disabled):not(.active) {
  border-color: #016bff;
  color: #016bff;
}

.page-btn:active:not(:disabled):not(.active) {
  transform: scale(0.95);
}

.page-btn.active {
  background: #016bff;
  border-color: #016bff;
  color: #ffffff;
  cursor: default;
}

.page-btn:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.page-ellipsis {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  font-size: 12.5px;
  color: #9ca3af;
  user-select: none;
}

.info-collapse-enter-active,
.info-collapse-leave-active {
  transition:
    max-height 0.28s cubic-bezier(0.4, 0, 0.2, 1),
    opacity 0.22s ease;
  overflow: hidden;
  max-height: 400px;
}

.info-collapse-enter-from,
.info-collapse-leave-to {
  max-height: 0;
  opacity: 0;
}
</style>