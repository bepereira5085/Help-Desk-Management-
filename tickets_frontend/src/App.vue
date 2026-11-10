<template>
  <router-view />
</template>

<script setup>
import { useQuasar } from 'quasar'

// Restaura a preferência de tema (claro/escuro)
const $q = useQuasar()
$q.dark.set($q.localStorage.getItem('helpdesk-dark') === true)
</script>
