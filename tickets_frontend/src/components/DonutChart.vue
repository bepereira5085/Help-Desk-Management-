<template>
  <q-card flat bordered class="full-height">
    <q-card-section class="text-subtitle1 text-weight-medium q-pb-none">
      {{ title }}
    </q-card-section>

    <q-card-section>
      <div v-if="total" class="donut-chart">
        <Doughnut :data="chartData" :options="chartOptions" :plugins="[centerTotalPlugin]" />
      </div>

      <div v-else class="donut-chart column items-center justify-center text-grey-6">
        <q-icon name="donut_large" size="48px" />
        <div class="q-mt-sm">Sem dados para exibir</div>
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { computed } from 'vue'
import { useQuasar } from 'quasar'
import { Doughnut } from 'vue-chartjs'
import { ArcElement, Chart as ChartJS, Legend, Tooltip } from 'chart.js'
import { resolveColor } from '@/utils/ticketMeta'

ChartJS.register(ArcElement, Tooltip, Legend)

// Gráfico de rosca genérico: items = [{ label, value, color }]
// A cor pode ser hex ou nome da paleta do Quasar (resolveColor converte para CSS).
const props = defineProps({
  title: { type: String, required: true },
  items: { type: Array, required: true }
})

const $q = useQuasar()

const total = computed(() => props.items.reduce((sum, item) => sum + item.value, 0))
const textColor = computed(() => ($q.dark.isActive ? '#ffffff' : '#424242'))

const chartData = computed(() => ({
  labels: props.items.map((item) => item.label),
  datasets: [
    {
      data: props.items.map((item) => item.value),
      backgroundColor: props.items.map((item) => resolveColor(item.color)),
      borderColor: $q.dark.isActive ? '#1d1d1d' : '#ffffff',
      borderWidth: 2
    }
  ]
}))

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  cutout: '65%',
  plugins: {
    legend: {
      position: 'bottom',
      labels: { color: textColor.value, usePointStyle: true, padding: 16 }
    },
    tooltip: {
      callbacks: {
        label: (context) => {
          const percent = total.value ? Math.round((context.parsed / total.value) * 100) : 0
          return ` ${context.label}: ${context.parsed} (${percent}%)`
        }
      }
    }
  }
}))

// Escreve o total no centro da rosca (lê total/cor no momento de desenhar)
const centerTotalPlugin = {
  id: 'centerTotal',
  afterDraw(chart) {
    const { ctx, chartArea } = chart
    const x = (chartArea.left + chartArea.right) / 2
    const y = (chartArea.top + chartArea.bottom) / 2

    ctx.save()
    ctx.textAlign = 'center'
    ctx.textBaseline = 'middle'
    ctx.fillStyle = textColor.value
    ctx.font = 'bold 28px Roboto, sans-serif'
    ctx.fillText(String(total.value), x, y - 8)
    ctx.font = '12px Roboto, sans-serif'
    ctx.fillText('chamados', x, y + 14)
    ctx.restore()
  }
}
</script>

<style scoped>
.donut-chart {
  position: relative;
  height: 280px;
}
</style>
