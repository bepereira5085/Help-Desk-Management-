<template>
  <q-card
    flat
    bordered
    v-ripple
    class="ticket-card cursor-pointer column full-height"
    :style="{ borderLeftColor: resolveColor(priority.color) }"
    @click="$emit('click', ticket)"
  >
    <q-card-section class="row items-center no-wrap q-pb-none">
      <StatusChip :label="status.label" :color="status.color" :icon="status.icon" dense />
      <q-space />
      <div class="text-caption text-grey-7 row items-center no-wrap">
        <q-icon name="schedule" class="q-mr-xs" />
        {{ formatDateTime(ticket.createdAt) }}
      </div>
    </q-card-section>

    <q-card-section class="q-pb-sm">
      <div class="text-caption text-grey-6">#{{ ticket.id }}</div>
      <div class="text-h6 ellipsis">{{ ticket.itemName }}</div>
      <div class="row items-center q-gutter-x-md text-caption text-grey-7">
        <span class="row items-center no-wrap">
          <q-icon name="apartment" class="q-mr-xs" :style="{ color: resolveColor(ticket.departmentColor) }" />
          {{ ticket.departmentName }}
        </span>
        <span class="row items-center no-wrap">
          <q-icon name="person" class="q-mr-xs" />
          {{ ticket.userName }}
        </span>
        <span v-if="ticket.equipmentTag" class="row items-center no-wrap">
          <q-icon name="qr_code_2" class="q-mr-xs" />
          {{ ticket.equipmentTag }}
        </span>
      </div>
    </q-card-section>

    <q-card-section class="q-pt-none col">
      <div class="ticket-card__description text-body2 text-grey-8">{{ ticket.description }}</div>
    </q-card-section>

    <q-card-section class="row items-center no-wrap q-pt-none">
      <div class="row q-gutter-xs">
        <StatusChip
          v-for="tag in ticket.tags"
          :key="tag.id"
          :label="tag.name"
          :color="tag.color"
          dense
        />
      </div>
      <q-space />
      <StatusChip :label="priority.label" :color="priority.color" :icon="priority.icon" dense />
    </q-card-section>
  </q-card>
</template>

<script setup>
import { computed } from 'vue'
import StatusChip from '@/components/StatusChip.vue'
import { PRIORITY, STATUS, formatDateTime, resolveColor } from '@/utils/ticketMeta'

const props = defineProps({
  ticket: { type: Object, required: true }
})

defineEmits(['click'])

const status = computed(() => STATUS[props.ticket.status])
const priority = computed(() => PRIORITY[props.ticket.priority])
</script>

<style scoped>
.ticket-card {
  border-left-width: 4px;
  border-left-style: solid;
  transition:
    transform 0.15s ease,
    box-shadow 0.15s ease;
}

.ticket-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.1);
}

.ticket-card__description {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.body--dark .ticket-card__description {
  color: #bdbdbd !important;
}
</style>
