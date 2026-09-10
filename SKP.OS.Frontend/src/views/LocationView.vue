<script setup lang="ts">
import { ref, computed, onMounted, watchEffect } from "vue";
import {
  IconInfoCircle,
  IconChevronDown,
  IconMapPinFilled,
  IconCheck,
  IconLogout,
  IconClock,
  IconAlertCircle,
} from "@tabler/icons-vue";
import { useCheckInStore } from "@/Stores/CheckInStore";
import { useStudentProfileStore } from "@/Stores/StudentProfileStore";
import { useRoomStore } from "@/Stores/RoomStore";
import type { RoomDto } from "@/types";

interface LocationOption {
  id: number;
  name: string;
  title?: string;
  subtitle?: string;
  description?: string;
}

const checkInStore = useCheckInStore();
const studentProfileStore = useStudentProfileStore();
const roomStore = useRoomStore();

const isLoading = ref(true);
const isSaving = ref(false);

const toastMessage = ref("");
const toastType = ref<"success" | "warning" | "info">("success");
let toastTimeout: number | undefined;

function showToast(message: string, type: "success" | "warning" | "info" = "success") {
  toastMessage.value = message;
  toastType.value = type;
  if (toastTimeout) clearTimeout(toastTimeout);
  toastTimeout = window.setTimeout(() => {
    toastMessage.value = "";
  }, 3500);
}

const INFO_EXPANDED_KEY = "location_info_expanded";
const infoExpanded = ref(localStorage.getItem(INFO_EXPANDED_KEY) !== "false");
watchEffect(() => {
  localStorage.setItem(INFO_EXPANDED_KEY, String(infoExpanded.value));
});

const availableLocations = ref<LocationOption[]>([]);

const isCheckedIn = ref(false);
const activeLocationName = ref<string | null>(null);
const activeLocationId = ref<number | null>(null);
const checkInTime = ref<Date | null>(null);
const activeCheckInId = ref<number | null>(null);

const checkInTimeFormatted = computed(() => {
  if (!checkInTime.value) return null;
  return checkInTime.value.toLocaleTimeString("da-DK", {
    hour: "2-digit",
    minute: "2-digit",
  });
});

function getLocationTitle(loc: LocationOption): string {
  if (loc.title) return loc.title;
  const parts = loc.name.split(" ");
  return parts[0] || loc.name;
}

function getLocationSubtitle(loc: LocationOption): string {
  if (loc.subtitle !== undefined) return loc.subtitle;
  const parts = loc.name.split(" ");
  return parts.slice(1).join(" ");
}

function isCurrentLocation(loc: LocationOption): boolean {
  if (!isCheckedIn.value) return false;
  if (activeLocationId.value && activeLocationId.value === loc.id) return true;
  return activeLocationName.value === loc.name;
}

async function handleSelectLocation(loc: LocationOption) {
  if (isSaving.value) return;

  if (isCheckedIn.value && isCurrentLocation(loc)) {
    showToast(`Du er allerede tjekket ind på ${loc.name}`, "info");
    return;
  }

  isSaving.value = true;
  const now = new Date();
  const prevCheckInId = activeCheckInId.value;
  const prevCheckInTime = checkInTime.value ? checkInTime.value.toISOString() : now.toISOString();
  const prevRoomId = activeLocationId.value || loc.id;
  const prevSeat = activeLocationName.value || loc.name;

  isCheckedIn.value = true;
  activeLocationName.value = loc.name;
  activeLocationId.value = loc.id;
  checkInTime.value = now;

  try {
    const profile = studentProfileStore.MY_STUDENT_PROFILE || (await studentProfileStore.GET_MY_STUDENT_PROFILE());
    if (profile?.id) {
      if (prevCheckInId) {
        await checkInStore.UPDATE_CHECK_IN(prevCheckInId, {
          roomId: prevRoomId,
          seat: prevSeat,
          checkInTime: prevCheckInTime,
          checkOutTime: now.toISOString(),
        });
      }

      const created = await checkInStore.CREATE_CHECK_IN({
        studentProfileId: profile.id,
        roomId: loc.id,
        seat: loc.name,
        checkInTime: now.toISOString(),
        checkOutTime: null,
      });

      if (created?.id) {
        activeCheckInId.value = created.id;
      }
    }
    showToast(`Du er nu tjekket ind på ${loc.name}`, "success");
  } catch (error) {
    const message = error instanceof Error ? error.message : `Kunne ikke tjekke ind på ${loc.name}`;
    showToast(message, "warning");
  } finally {
    isSaving.value = false;
  }
}

async function handleCheckOut() {
  if (isSaving.value) return;
  if (!isCheckedIn.value) {
    showToast("Du er ikke tjekket ind", "info");
    return;
  }

  isSaving.value = true;
  const now = new Date();
  const prevCheckInId = activeCheckInId.value;
  const prevRoomId = activeLocationId.value || 1;
  const prevSeat = activeLocationName.value || "Tjek ud";
  const prevTime = checkInTime.value ? checkInTime.value.toISOString() : now.toISOString();

  isCheckedIn.value = false;
  activeLocationName.value = null;
  activeLocationId.value = null;
  checkInTime.value = null;
  activeCheckInId.value = null;

  try {
    if (prevCheckInId) {
      await checkInStore.UPDATE_CHECK_IN(prevCheckInId, {
        roomId: prevRoomId,
        seat: prevSeat,
        checkInTime: prevTime,
        checkOutTime: now.toISOString(),
      });
    }
    showToast("Du er nu tjekket ud. God fyraften!", "info");
  } catch {
    showToast("Du er nu tjekket ud. God fyraften!", "info");
  } finally {
    isSaving.value = false;
  }
}

onMounted(async () => {
  try {
    const [profile, rooms] = await Promise.all([
      studentProfileStore.GET_MY_STUDENT_PROFILE(),
      roomStore.GET_ROOMS(),
    ]);

    availableLocations.value = (rooms ?? []).map((r: RoomDto) => {
      let title = r.name;
      let subtitle = r.location || "";
      if (!r.location && r.name.includes(" ")) {
        const parts = r.name.split(" ");
        title = parts[0];
        subtitle = parts.slice(1).join(" ");
      }
      return {
        id: r.id,
        name: r.name,
        title,
        subtitle,
        description: r.location || "Arbejdsområde",
      };
    });

    if (profile?.id) {
      const checkIns = await checkInStore.GET_CHECK_INS(profile.id);
      const current = checkIns.find((c) => !c.checkOutTime);

      if (current) {
        isCheckedIn.value = true;
        activeLocationId.value = current.roomId;
        activeLocationName.value = current.room?.name || current.seat;
        checkInTime.value = new Date(current.checkInTime);
        activeCheckInId.value = current.id;
      }
    }
  } catch {
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <div class="header-titles">
        <h1 class="page-title">Hvor er jeg nu</h1>
      </div>
    </div>

    <Transition name="toast">
      <div
        v-if="toastMessage"
        class="toast-notification"
        :class="toastType"
        role="status"
      >
        <IconCheck v-if="toastType === 'success'" :size="16" :stroke-width="2.5" />
        <IconAlertCircle v-else-if="toastType === 'warning'" :size="16" :stroke-width="2.5" />
        <IconInfoCircle v-else :size="16" :stroke-width="2.5" />
        <span>{{ toastMessage }}</span>
      </div>
    </Transition>

    <template v-if="isLoading">
      <div class="skeleton-surface" />
    </template>

    <template v-else>
      <div class="surface">
        <section class="section section-info">
          <button
            class="info-toggle"
            :aria-expanded="infoExpanded"
            type="button"
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
                <strong>HUSK:</strong> Den normale arbejdstid er 37 timer per uge. Mandag til torsdag i et tidsrum fra kl. 08.00–15.30 og fredag fra kl. 8.00–14.30.
              </p>

              <ul class="info-list">
                <li>
                  Du skal tjekke ind hver morgen senest kl. 8:05 og tjekke ud kl. 15:30 (fredag kl. 14:30). Dette gør du ved at logge på opgavesystemet og vælge den knap nedenfor, der svarer til din aktuelle placering, f.eks. MU7 zone-5.
                </li>
                <li>
                  Tjek ud udføres ved at klikke på knappen <em>"Tjek ud / Fyraften"</em>.
                </li>
                <li>
                  Forlader du området, skal du vælge en ny placering (gælder ikke toiletbesøg o.l.).
                </li>
                <li>
                  Går du hjem tidligere (fri eller sygdom), skal du også tjekke ud og huske at underrette din instruktør.
                </li>
              </ul>

              <p class="info-note">
                <strong>OBS:</strong> Det er DIT ansvar at tjekke ind/ud hver dag og underrette din instruktør, når du skal holde fri, bliver pludselig syg eller holder fyraften!
              </p>
            </div>
          </Transition>
        </section>

        <div class="divider" />

        <section class="section section-status">
          <div class="status-container">
            <div class="status-left">
              

              <div class="status-details">
                <div class="status-location-row">
                  <span class="status-caption">Aktuel placering:</span>
                  <span class="status-location-name" :class="{ 'none': !isCheckedIn }">
                    {{ isCheckedIn ? activeLocationName : 'Ingen placering valgt' }}
                  </span>
                </div>

                <div v-if="isCheckedIn && checkInTimeFormatted" class="status-time-row">
                  <IconClock :size="14" :stroke-width="2.2" />
                  <span>Tjekket ind kl. {{ checkInTimeFormatted }}</span>
                </div>
              </div>
            </div>

            <div class="status-right">
              <button
                class="checkout-btn"
                :class="{ 'btn-disabled': !isCheckedIn }"
                type="button"
                :disabled="!isCheckedIn || isSaving"
                @click="handleCheckOut"
              >
                <IconLogout :size="16" :stroke-width="2.2" />
                <span>Tjek ud / Fyraften</span>
              </button>
            </div>
          </div>
        </section>

        <div class="divider" />

        <section class="section section-locations">
          <div class="locations-section-header">
            <div class="locations-icon-badge">
              <IconMapPinFilled :size="20" />
            </div>
            <div class="locations-title-block">
              <h2 class="locations-title">Vælg din placering</h2>
              <p class="locations-subtitle">Klik på den placering, der passer til, hvor du er lige nu.</p>
            </div>
          </div>

          <div class="locations-grid">
            <button
              v-for="loc in availableLocations"
              :key="loc.id"
              class="location-card"
              :class="{ 'selected': isCurrentLocation(loc) }"
              type="button"
              :disabled="isSaving"
              :title="loc.description || loc.name"
              @click="handleSelectLocation(loc)"
            >
              <div class="card-left">
                <div
                  class="pin-badge"
                  :class="{ 'active': isCurrentLocation(loc) }"
                >
                  <IconMapPinFilled :size="26" />
                </div>
                <div class="card-texts">
                  <span class="card-title">{{ getLocationTitle(loc) }}</span>
                  <span class="card-subtitle">{{ getLocationSubtitle(loc) }}</span>
                </div>
              </div>

              <div
                v-if="isCurrentLocation(loc)"
                class="card-selected-badge"
                title="Aktiv placering"
              >
                <IconCheck :size="15" :stroke-width="3" />
              </div>
            </button>
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
  position: relative;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 12px;
}

.header-titles {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.page-title {
  font-size: 24px;
  font-weight: 700;
  color: #111827;
  letter-spacing: -0.5px;
}

.page-subtitle {
  font-size: 13.5px;
  color: #6b7280;
  font-weight: 400;
}

.breadcrumbs {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  font-size: 12.5px;
  color: #9ca3af;
  user-select: none;
}

.breadcrumb-item {
  color: #6b7280;
  font-weight: 500;
}

.breadcrumb-item.active {
  color: #9ca3af;
  font-weight: 600;
}

.breadcrumb-sep {
  font-size: 11px;
  color: #cbd5e1;
}

.toast-notification {
  position: fixed;
  top: 70px;
  right: 28px;
  z-index: 100;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  pointer-events: none;
}

.toast-notification.success {
  background: #10b981;
  color: #ffffff;
}

.toast-notification.info {
  background: #016bff;
  color: #ffffff;
}

.toast-notification.warning {
  background: #b45309;
  color: #ffffff;
}

.toast-enter-active,
.toast-leave-active {
  transition: all 0.25s ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

.surface {
  background: #ffffff;
  border: 1px solid #dde1e5;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
  overflow: hidden;
}

.skeleton-surface {
  height: 520px;
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

.divider {
  height: 1px;
  background: #dde1e5;
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

.section-status {
  background: #ffffff;
  padding: 16px 22px;
}

.status-container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 16px;
}

.status-left {
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.status-indicator-badge {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  padding: 5px 11px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  user-select: none;
}

.status-indicator-badge.active {
  background: #ecfdf5;
  color: #065f46;
  border: 1px solid #a7f3d0;
}

.status-indicator-badge.inactive {
  background: #f1f5f9;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.status-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  flex-shrink: 0;
}

.status-indicator-badge.active .status-dot {
  background: #10b981;
  box-shadow: 0 0 0 2px rgba(16, 185, 129, 0.25);
  animation: pulse 2s infinite;
}

.status-indicator-badge.inactive .status-dot {
  background: #94a3b8;
}

@keyframes pulse {
  0% {
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.5);
  }
  70% {
    box-shadow: 0 0 0 5px rgba(16, 185, 129, 0);
  }
  100% {
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0);
  }
}

.status-details {
  display: flex;
  align-items: center;
  gap: 14px;
  flex-wrap: wrap;
}

.status-location-row {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13.5px;
}

.status-caption {
  color: #6b7280;
  font-weight: 500;
}

.status-location-name {
  color: #111827;
  font-weight: 700;
}

.status-location-name.none {
  color: #9ca3af;
  font-style: italic;
  font-weight: 500;
}

.status-time-row {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 12px;
  font-weight: 500;
  color: #6b7280;
  background: #f8fafc;
  padding: 3px 8px;
  border-radius: 5px;
  border: 1px solid #e2e8f0;
}

.checkout-btn {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  height: 38px;
  padding: 0 16px;
  background-color: #ef4444;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  outline: none;
  user-select: none;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
  transition:
    background-color 0.18s ease,
    box-shadow 0.18s ease,
    transform 0.12s ease;
}

.checkout-btn:hover:not(:disabled) {
  background-color: #dc2626;
  box-shadow: 0 3px 8px rgba(239, 68, 68, 0.25);
  transform: translateY(-1px);
}

.checkout-btn:active:not(:disabled) {
  transform: translateY(0);
}

.checkout-btn:focus-visible {
  outline: 2px solid #ef4444;
  outline-offset: 2px;
}

.checkout-btn.btn-disabled,
.checkout-btn:disabled {
  background-color: #f1f5f9;
  color: #94a3b8;
  border: 1px solid #e2e8f0;
  cursor: not-allowed;
  box-shadow: none;
  transform: none;
}

.section-locations {
  padding: 24px;
}

.locations-section-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
}

.locations-icon-badge {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 38px;
  height: 38px;
  border-radius: 50%;
  background: #e0f2fe;
  color: #0284c7;
  flex-shrink: 0;
}

.locations-title-block {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.locations-title {
  font-size: 15px;
  font-weight: 700;
  color: #111827;
  letter-spacing: -0.2px;
}

.locations-subtitle {
  font-size: 12.5px;
  color: #6b7280;
}

.locations-grid {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 14px;
}

.location-card {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: space-between;
  min-height: 94px;
  padding: 16px 20px;
  background: #ffffff;
  border: 1.5px solid #e5e7eb;
  border-radius: 12px;
  cursor: pointer;
  text-align: left;
  font-family: inherit;
  outline: none;
  transition:
    border-color 0.2s ease,
    background-color 0.2s ease,
    box-shadow 0.2s ease,
    transform 0.15s ease;
}

.location-card:hover:not(:disabled) {
  border-color: #93c5fd;
  background-color: #f8fafc;
  box-shadow: 0 3px 10px rgba(0, 0, 0, 0.05);
  transform: translateY(-2px);
}

.location-card:active:not(:disabled) {
  transform: translateY(0);
}

.location-card:focus-visible {
  outline: 2px solid #016bff;
  outline-offset: 2px;
}

.location-card.selected {
  background-color: #f0fdf4;
  border-color: #86efac;
  box-shadow: 0 2px 8px rgba(16, 185, 129, 0.12);
}

.location-card.selected:hover {
  background-color: #ecfdf5;
  border-color: #4ade80;
}

.card-left {
  display: flex;
  align-items: center;
  gap: 16px;
  min-width: 0;
}

.pin-badge {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 52px;
  height: 52px;
  border-radius: 50%;
  background: #fee2e2;
  color: #dc2626;
  flex-shrink: 0;
  transition:
    background-color 0.2s ease,
    color 0.2s ease;
}

.pin-badge.active {
  background: #dcfce7;
  color: #16a34a;
}

.card-texts {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.card-title {
  font-size: 15.5px;
  font-weight: 700;
  color: #111827;
  line-height: 1.25;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.card-subtitle {
  font-size: 14px;
  font-weight: 700;
  color: #111827;
  line-height: 1.25;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.card-selected-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: #22c55e;
  color: #ffffff;
  flex-shrink: 0;
  box-shadow: 0 1px 3px rgba(34, 197, 94, 0.4);
}

@media (max-width: 1200px) {
  .locations-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .breadcrumbs {
    display: none;
  }

  .status-container {
    flex-direction: column;
    align-items: flex-start;
  }

  .status-right {
    width: 100%;
  }

  .checkout-btn {
    width: 100%;
    justify-content: center;
  }

  .locations-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 500px) {
  .locations-grid {
    grid-template-columns: 1fr;
  }
}
</style>
